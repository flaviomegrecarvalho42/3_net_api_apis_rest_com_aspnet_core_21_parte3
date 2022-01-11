using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Alura.ListaLeitura.Api.Models
{
    public class ErrorResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public ErrorResponse InnerError { get; set; }
        public string[] Detalhes { get; set; }

        public static ErrorResponse From(Exception error)
        {
            if (error == null)
            {
                return null;
            }

            return new ErrorResponse
            {
                Code = error.HResult,
                Message = error.Message,
                InnerError = From(error.InnerException)
            };
        }

        public static ErrorResponse FromModelState(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(m => m.Errors).ToList();

            return new ErrorResponse
            {
                Code = 400,
                Message = "Houve(rão) erro(s) no envio da requisição.",
                Detalhes = erros.Select(e => e.ErrorMessage).ToArray()
            };
        }
    }
}
