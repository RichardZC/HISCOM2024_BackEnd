using Domain.Models;

namespace Admin.Indexation
{
    public class ParameterIvm
    {
        public const string indexUid = "parameter";
        
        public static ParameterIvm GetParameterIvm(Parametro parameter)
        {

            ParameterIvm p = new ParameterIvm();
            p.Id = parameter.Id.ToString();
            p.Llave = parameter.Llave;
            p.Valor = parameter.Valor;
            return p;
        }
        
        public string Id { get; set; }
        public string Llave { get; set; }
        public string Valor { get; set; }
    }
}