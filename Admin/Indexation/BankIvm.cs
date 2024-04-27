using Domain.Models;


namespace Admin.Indexation
{
    public class BankIvm
    {
        public const string indexUid = "bank";
        public static BankIvm GetBankIvm(Banco bank)
        {
            BankIvm b = new BankIvm();
            b.Id = bank.Id.ToString();
            b.Nombre = bank.Nombre;
            b.Abreviacion = bank.Abreviacion;
            return b;
        }

        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Abreviacion { get; set; }
    }
}
