using Domain.Models;

namespace Admin.Indexation
{
    public class TurnIvm
    {
        public const string indexUid = "turn";
        
        public static TurnIvm GetTurnIvm(Turno turn)
        {
            TurnIvm t = new TurnIvm();
            t.Id = turn.Id.ToString();
            t.Denominacion = turn.Denominacion;
            t.Descripcion = turn.Descripcion;
            t.Horas = turn.Horas;
            return t;
        }
        
        public string Id { get; set; }
        public string Denominacion { get; set; }
        public string Descripcion { get; set; }
        public int Horas { get; set; }

    }
}