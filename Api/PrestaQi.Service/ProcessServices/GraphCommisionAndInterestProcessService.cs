using DocumentFormat.OpenXml.Drawing.Charts;
using InsiscoCore.Service;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.Text;

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
