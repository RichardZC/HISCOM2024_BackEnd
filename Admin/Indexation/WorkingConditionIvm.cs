using Domain.Models;

namespace Admin.Indexation
{
    public class WorkingConditionIvm
    {
        public const string indexUid = "working-condition";
        
        public static WorkingConditionIvm GetWorkingConditionIvm(CondicionLaboral workingCondition)
        {
            WorkingConditionIvm ct = new WorkingConditionIvm();
            ct.Id = workingCondition.Id;
            ct.Denominacion = workingCondition.Denominacion;
            ct.TotalHoras = workingCondition.TotalHoras;
            return ct;
        }
        
        public string Id { get; set; }
        public string Denominacion { get; set; }
        public int TotalHoras { get; set; }
    }
}