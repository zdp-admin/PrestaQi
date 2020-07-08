using InsiscoCore.Base.Service;
using InsiscoCore.Service;
using MoreLinq;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using PrestaQi.Model.Dto.Output;
using PrestaQi.Service.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;

namespace PrestaQi.Service.ProcessServices
{
    public class LiquidityProcessService : ProcessService<Liquidity>
    {
        IRetrieveService<Capital> _CapitalRetrieveService;
        IRetrieveService<Investor> _InvestorRetrieveService;
        IRetrieveService<Advance> _AdvanceRetrieveService;

        public LiquidityProcessService(
            IRetrieveService<Capital> capitalRetrieveService,
            IRetrieveService<Investor> investorRetrieveService,
            IRetrieveService<Advance> advanceRetrieveService
            )
        {
            this._CapitalRetrieveService = capitalRetrieveService;
            this._InvestorRetrieveService = investorRetrieveService;
            this._AdvanceRetrieveService = advanceRetrieveService;
        }

        public Liquidity ExecuteProcess(GetLiquidity getLiquidity)
        {
            if (getLiquidity.Filter != "range-dates" && getLiquidity.Filter != "specific-day")
            {
                var resultDate = Utilities.CalcuateDates(getLiquidity.Filter);
                getLiquidity.Start_Date = resultDate.Item1;
                getLiquidity.End_Date = resultDate.Item2;
                getLiquidity.Is_Specifid_Day = resultDate.Item3;
            }

            if (getLiquidity.Start_Date.Date == getLiquidity.End_Date.Date && getLiquidity.Is_Specifid_Day == false)
                getLiquidity.Is_Specifid_Day = true;

            if (getLiquidity.Is_Specifid_Day)
                getLiquidity.End_Date = getLiquidity.Start_Date;

            var capitals = GetCapitals(getLiquidity);
            var investors = GetInvestor(getLiquidity, capitals.Select(p => p.investor_id).ToList());
            var advances = GetAdvances(getLiquidity);

            List<LiquidityDetail> liquidityDetails = new List<LiquidityDetail>();

            if (capitals.Count != 0 || investors.Count != 0 || advances.Count != 0)
            {
                while (getLiquidity.Start_Date.Date <= getLiquidity.End_Date.Date)
                {
                    LiquidityDetail liquidityDetail = new LiquidityDetail()
                    {
                        Date = getLiquidity.Start_Date,
                        Amount_Call_Capital = capitals.Where(p => p.created_at.Date == getLiquidity.Start_Date.Date).Sum(p => p.Amount),
                        Amount_Capital = investors.Where(p => p.created_at.Date == getLiquidity.Start_Date.Date).Sum(p => p.Total_Amount_Agreed),
                        Amount_Advance = advances.Where(p => p.Date_Advance.Date == getLiquidity.Start_Date.Date).Sum(p => p.Amount)
                    };

                    liquidityDetails.Add(liquidityDetail);

                    getLiquidity.Start_Date = getLiquidity.Start_Date.AddDays(1);
                }
            }

            Liquidity liquidity = new Liquidity();
            liquidity.LiquidityDetails = liquidityDetails;

            liquidity.Call_Capital = liquidityDetails.Sum(p => p.Amount_Call_Capital);
            liquidity.Commited_Capital = liquidityDetails.Sum(p => p.Amount_Capital);
            liquidity.Total_Advances = liquidityDetails.Sum(p => p.Amount_Advance);

            if (liquidity.Call_Capital > 0)
                liquidity.Average_Call_Capital = getLiquidity.Is_Specifid_Day ? Math.Round(liquidity.Call_Capital / capitals.Count, 2) :
                    Math.Round(liquidity.Call_Capital / liquidityDetails.Count, 2);

            if (liquidity.Commited_Capital > 0)
                liquidity.Average_Capital = getLiquidity.Is_Specifid_Day ? Math.Round(liquidity.Commited_Capital / investors.Count, 2) :
                    Math.Round(liquidity.Commited_Capital / liquidityDetails.Count, 2);

            if (liquidity.Total_Advances > 0)
                liquidity.Average_Advances = getLiquidity.Is_Specifid_Day ? Math.Round(liquidity.Total_Advances / advances.Count, 2) :
                    Math.Round(liquidity.Total_Advances / liquidityDetails.Count, 2);

            return liquidity;
        }

        public List<Capital> GetCapitals(GetLiquidity getLiquidity)
        {
            List<Capital> capitals = new List<Capital>();

            if (getLiquidity.Is_Specifid_Day)
                capitals = this._CapitalRetrieveService.Where(p => p.created_at.Date == getLiquidity.Start_Date.Date).ToList();
            else
            {
                capitals = this._CapitalRetrieveService.Where(p => p.created_at.Date >= getLiquidity.Start_Date.Date &&
                p.created_at.Date <= getLiquidity.End_Date).ToList();
            }

            return capitals;
        }

        public List<Investor> GetInvestor(GetLiquidity getLiquidity, List<int> investorIds)
        {
            List<Investor> investors = new List<Investor>();

            if (getLiquidity.Is_Specifid_Day)
                investors = this._InvestorRetrieveService.Where(p => p.created_at.Date == getLiquidity.Start_Date.Date).ToList();
            else
            {
                investors = this._InvestorRetrieveService.Where(p => p.created_at.Date >= getLiquidity.Start_Date.Date &&
                p.created_at.Date <= getLiquidity.End_Date).ToList();
            }

            var investorsAux = this._InvestorRetrieveService.Where(p => investorIds.Contains(p.id)).ToList();

            investors.AddRange(investorsAux);

            return investors.DistinctBy(p => p.id).ToList();
        }

        public List<Advance> GetAdvances(GetLiquidity getLiquidity)
        {
            List<Advance> advances = new List<Advance>();

            if (getLiquidity.Is_Specifid_Day)
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date == getLiquidity.Start_Date.Date).ToList();
            else
            {
                advances = this._AdvanceRetrieveService.Where(p => p.Date_Advance.Date >= getLiquidity.Start_Date.Date &&
                p.Date_Advance.Date <= getLiquidity.End_Date).ToList();
            }

            return advances;
        }
    }
}
