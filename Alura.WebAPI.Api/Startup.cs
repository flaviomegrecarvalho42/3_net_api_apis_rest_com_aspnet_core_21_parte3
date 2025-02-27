﻿using Alura.ListaLeitura.Api.Formatters;
using Alura.ListaLeitura.Modelos;
using Alura.ListaLeitura.Persistencia;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Alura.ListaLeitura.Api.Filters;
using System.Text;

namespace Alura.WebAPI.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<LeituraContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("ListaLeitura"));
            });

            services.AddTransient<IRepository<Livro>, RepositorioBaseEF<Livro>>();

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new LivroCsvFormatter());
                //filtro de exceção
                options.Filters.Add(typeof(ErroResponseExceptionFilter));
            }).AddXmlSerializerFormatters();

            services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes(true)
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v.ToString()}" == docName);
                });

                options.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey",
                    Description = "Autenticação Bearer via JWT"
                });
                options.AddSecurityRequirement(
                    new Dictionary<string, IEnumerable<string>> {
                        { "Bearer", new string[] { } }
                });

                options.EnableAnnotations();
                options.DescribeAllEnumsAsStrings();
                options.DescribeStringEnumsInCamelCase();

                options.DocumentFilter<TagDescriptionsDocumentFilter>();
                options.OperationFilter<AuthResponsesOperationFilter>();
                options.OperationFilter<AddInfoToParamVersionOperationFilter>();

                options.SwaggerDoc("v1.0", new Info { Title = "Lista de Leitura API - v1.0", Version = "1.0" });
                options.SwaggerDoc("v2.0", new Info { Title = "Lista de Leitura API - v2.0", Description = "API com serviços relacionados às listas de leitura, produzidas para a Alura.", Version = "2.0" });
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            #region Configuração de versionamento via rota
            services.AddApiVersioning();
            #endregion

            #region Confirguração de versionamenteo via Query String ou Header
            //services.AddApiVersioning(options =>
            //{
            //    options.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"),
            //                                                        new HeaderApiVersionReader("api-version"));
            //});
            #endregion

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            }).AddJwtBearer("JwtBearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("alura-webapi-authentication-valid")),
                    ClockSkew = TimeSpan.FromMinutes(5),
                    ValidIssuer = "Alura.WebApp",
                    ValidAudience = "Postman",
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder.WithOrigins("http://localhost"));

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Version 1.0");
                c.SwaggerEndpoint("/swagger/v2.0/swagger.json", "Version 2.0");
            });

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
