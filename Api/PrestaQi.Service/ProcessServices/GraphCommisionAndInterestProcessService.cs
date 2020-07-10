using DocumentFormat.OpenXml.Drawing.Charts;
using InsiscoCore.Service;
using PrestaQi.Model.Dto.Input;

namespace PrestaQi.Service.ProcessServices
{
    public class GraphCommisionAndInterestProcessService : ProcessService<GraphCommisionAndInterest>
    {
        public GraphCommisionAndInterestProcessService()
        {

        }

        public bool ExecuteProcess(GraphCommisionAndInterest graphCommisionAndInterest)
        {
            Chart chart = new Chart();
            

            return true;
        }
    }
}
