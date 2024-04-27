using Domain.Models;

namespace Admin.Indexation
{
    public class PositionIvm
    {
        public const string indexUid = "position";
        
        public static PositionIvm GetPositionIvm(Cargo position)
        {
            PositionIvm p = new PositionIvm();
            p.Id = position.Id;
            p.Denominacion = position.Denominacion;
            return p;
        }
        
        public string Id { get; set; }
        public string Denominacion { get; set; }
        public int Total { get; set; }
    }
}