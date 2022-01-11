namespace Alura.ListaLeitura.WebApp.Util
{
    public static class Helpers
    {
        public static string EnvolverComAspasDuplas(string texto)
        {
            return $"\u0022{texto}\u0022"; ;
        }
    }
}
