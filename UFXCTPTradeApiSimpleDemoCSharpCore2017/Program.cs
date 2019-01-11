/*******************************************************
* UFXCTPTradeApiSimpleDemoCSharpCore2017
* www.xfinapi.com
*******************************************************/
using System;
using System.Threading;
using XFinApi.TradeApi;

namespace UFXCTPTradeApiSimpleDemoCSharpCore2017
{
    class Program
    {
        //////////////////////////////////////////////////////////////////////////////////
        //配置信息
        class Config
        {
            //注册UFXCTP模拟账户:QQ群586525357

            //地址账户
            public string MarketAddress = "";
            public string TradeAddress = "";
            public string UserName = "";//公用测试账户。为了测试准确，请注册使用您自己的账户。
            public string Password = "";
            public string LicenseFile = "license.dat";
            public string LicensePwd = "";
            public string SafeLevel = "";
            public string CertFile = "";
            public string CertPwd = "";


            //合约
            public string ExchangeID = "1";//上证:1, 深证:2, 郑商:F1, 大商:F2, 上期:F3, 中金:F4, 能源中心:F5
            public string InstrumentID = "10001505";

            //行情
            public double SellPrice1 = -1;
            public double BuyPrice1 = -1;
        };

        //////////////////////////////////////////////////////////////////////////////////
        // 变量
        static Config Cfg = new Config();
        static IMarket market;
        static ITrade trade;
        static MarketEvent marketEvent;
        static TradeEvent tradeEvent;

        //////////////////////////////////////////////////////////////////////////////////
        // 输出
        static void PrintNotifyInfo(NotifyParams param)
        {
            string strs = "";
            for (int i = 0; i < param.CodeInfos.Count; i++)
            {
                CodeInfo info = param.CodeInfos[i];
                strs += "(Code=" + info.Code +
            ";LowerCode=" + info.LowerCode +
            ";LowerMessage=" + info.LowerMessage + ")";
            }

            Console.WriteLine(string.Format(" OnNotify: Action={0:d}, Result={1:d}{2}", param.ActionType, param.ResultType, strs));
        }

        static void PrintSubscribedInfo(QueryParams instInfo)
        {
            Console.WriteLine(string.Format("- OnSubscribed: {0}", instInfo.InstrumentID));
        }

        static void PrintUnsubscribedInfo(QueryParams instInfo)
        {
            Console.WriteLine(string.Format("- OnUnsubscribed: {0}", instInfo.InstrumentID));
        }

        static void PrintTickInfo(Tick tick)
        {
            Console.WriteLine(string.Format(" Tick,{0}, HighestPrice={1:g}, LowestPrice={2:g}, BidPrice0={3:g}, BidVolume0={4:d}, AskPrice0={5:g}, AskVolume0={6:d}, LastPrice={7:g}, TotalVolume={8:d}, TradingTime={9}",
                tick.InstrumentID,
                tick.HighestPrice,
                tick.LowestPrice,
                tick.GetBidPrice(0),
                tick.GetBidVolume(0),
                tick.GetAskPrice(0),
                tick.GetAskVolume(0),
                tick.LastPrice,
                tick.TotalVolume,
                tick.TradingTime));
        }

        static void PrintOrderInfo(Order order)
        {
            Console.WriteLine(string.Format("  Ref={0}, ID={1}, InstID={2}, Price={3:g}, Volume={4:d}, NoTradedVolume={5:d}, Direction={6:d}, PriceCond={7:d}, SessionID={8:d}, Status={9:d}, Msg={10}, {11}",
                order.OrderRef, order.OrderID,
                order.InstrumentID, order.Price, order.Volume, order.NoTradedVolume,
                order.Direction,
                order.PriceCond,
                order.SessionID,
                order.Status,
                order.StatusMsg,
                order.OrderTime
                ));
        }

        static void PrintTradeInfo(TradeOrder trade)
        {
            Console.WriteLine(string.Format("  ID={0}, OrderRef={1}, InstID={2}, Price={3:g}, Volume={4:d}, Direction={5:d}, {6}",
                trade.TradeID, trade.OrderID,
                trade.InstrumentID, trade.Price, trade.Volume,
                trade.Direction,
                trade.TradeTime));
        }

        static void PrintInstrumentInfo(Instrument inst)
        {
            Console.WriteLine(string.Format(" ExchangeID={0}, ProductID={1}, ID={2}, Name={3}",
                inst.ExchangeID, inst.ProductID,
                inst.InstrumentID, inst.InstrumentName));
        }

        static void PrintPositionInfo(Position pos)
        {
            long positionTotal = ITradeApi.IsDefaultValue(pos.PositionTotal) ? 0 : pos.PositionTotal;
            long positionSellable = ITradeApi.IsDefaultValue(pos.PositionSellable) ? 0 : pos.PositionSellable;
            long positionYesterday = ITradeApi.IsDefaultValue(pos.PositionYesterday) ? 0 : pos.PositionYesterday;
            double avgPrice = ITradeApi.IsDefaultValue(pos.AvgPrice) ? 0 : pos.AvgPrice;
            Console.WriteLine(string.Format(" InstID={0}, Direction={1:d}, PositionTotal={2:d}, PositionSellable={3:d}, PositionYesterday={4:d}, AvgPrice={5:g}",
                pos.InstrumentID, pos.Direction,
                positionTotal, positionSellable, positionYesterday, avgPrice));
        }

        static void PrintAccountInfo(Account acc)
        {
            Console.WriteLine(string.Format("  Balance={0:g}, Available={1:g}, CloseProfit={2:g}, Commission={3:g}, PositionProfit={4:g}, CurrMargin={5:g}\n",
                acc.Balance, acc.Available, acc.CloseProfit, acc.Commission, acc.PositionProfit, acc.CurrMargin));
        }

        //////////////////////////////////////////////////////////////////////////////////
        //API 创建失败错误码的含义，其他错误码的含义参见XTA_W32\Cpp\ApiEnum.h文件
        static string[] StrCreateErrors = {
            "无错误",
            "头文件与接口版本不匹配",
            "头文件与实现版本不匹配",
            "实现加载失败",
            "实现入口未找到",
            "创建实例失败",
            "无授权文件",
            "授权版本不符",
            "最后一次通信超限",
            "机器码错误",
            "认证文件到期",
            "认证超时"

        };

        //////////////////////////////////////////////////////////////////////////////////
        //行情事件
        public class MarketEvent : MarketListener
        {
            public override void OnNotify(NotifyParams notifyParams)
            {

                Console.WriteLine("* Market");

                PrintNotifyInfo(notifyParams);

                //连接成功后可订阅合约
                if ((int)XFinApi.TradeApi.ActionKind.Open == notifyParams.ActionType &&
                    (int)ResultKind.Success == notifyParams.ResultType && market != null)
                {
                    //订阅
                    QueryParams param = new QueryParams();
                    param.ExchangeID = Cfg.ExchangeID;
                    param.InstrumentID = Cfg.InstrumentID;
                    market.Subscribe(param);
                }

                //ToDo ...
            }

            public override void OnSubscribed(QueryParams instInfo)
            {

                PrintSubscribedInfo(instInfo);

                //ToDo ...
            }

            public override void OnUnsubscribed(QueryParams instInfo)
            {

                PrintUnsubscribedInfo(instInfo);

                //ToDo ...
            }

            public override void OnTick(Tick tick)
            {
                if (Cfg.SellPrice1 <= 0 && Cfg.BuyPrice1 <= 0)

                    PrintTickInfo(tick);

                Cfg.SellPrice1 = tick.GetAskPrice(0);
                Cfg.BuyPrice1 = tick.GetBidPrice(0);

                //ToDo ...
            }
        };

        //////////////////////////////////////////////////////////////////////////////////
        //交易事件
        public class TradeEvent : TradeListener
        {
            public override void OnNotify(NotifyParams notifyParams)
            {

                Console.WriteLine("* Trade");

                PrintNotifyInfo(notifyParams);

                //ToDo ...
            }

            public override void OnUpdateOrder(Order order)
            {

                Console.WriteLine("- OnUpdateOrder:");

                PrintOrderInfo(order);

                //ToDo ...
            }

            public override void OnUpdateTradeOrder(TradeOrder trade)
            {

                Console.WriteLine("- OnUpdateTradeOrder:");

                PrintTradeInfo(trade);

                //ToDo ...
            }

            public override void OnQueryOrder(OrderList orders)
            {

                Console.WriteLine("- OnQueryOrder:");

                for (int i = 0; i < orders.Count; i++)
                {
                    Order order = orders[i];
                    PrintOrderInfo(order);

                    //ToDo ...
                }
            }

            public override void OnQueryTradeOrder(TradeOrderList trades)
            {

                Console.WriteLine("- OnQueryTradeOrder:");

                for (int i = 0; i < trades.Count; i++)
                {
                    TradeOrder trade = trades[i];
                    PrintTradeInfo(trade);

                    //ToDo ...
                }
            }

            public override void OnQueryInstrument(InstrumentList insts)
            {

                Console.WriteLine("- OnQueryInstrument:");

                for (int i = 0; i < insts.Count; i++)
                {
                    Instrument inst = insts[i];
                    PrintInstrumentInfo(inst);

                    //ToDo ...
                }
            }

            public override void OnQueryPosition(PositionList posInfos)
            {
                Console.WriteLine("- OnQueryPosition:");
                for (int i = 0; i < posInfos.Count; i++)
                {
                    Position pos = posInfos[i];
                    PrintPositionInfo(pos);
                }

                //ToDo ...
            }

            public override void OnQueryAccount(Account accInfo)
            {
                Console.WriteLine("- OnQueryAccount:");

                PrintAccountInfo(accInfo);

                //ToDo ...
            }
        };

        //////////////////////////////////////////////////////////////////////////////////
        //行情测试
        static void MarketTest()
        {
            //创建 IMarket
            // char* path 指 xxx.exe 同级子目录中的 xxx.dll 文件
            int err = -1;

            market = ITradeApi.XFinApi_CreateMarketApi("XTA_W32/Api/UFXCTP_V3.7.1.10/XFinApi.UFXCTPTradeApi.dll", out err);


            if (err > 0 || market == null)
            {
                Console.WriteLine(string.Format("* Market XFinApiCreateError={0};", StrCreateErrors[err]));
                return;
            }

            //注册事件
            marketEvent = new MarketEvent();
            market.SetListener(marketEvent);

            //连接服务器
            OpenParams openParams = new OpenParams();
            openParams.HostAddress = Cfg.MarketAddress;
            openParams.UserID = Cfg.UserName;
            openParams.Password = Cfg.Password;
            openParams.Configs.Add("LicenseFile", Cfg.LicenseFile);
            openParams.IsUTF8 = true;
            market.Open(openParams);

            /*
            连接成功后才能执行订阅行情等操作，检测方法有两种：
            1、IMarket.IsOpened()=true
            2、MarketListener.OnNotify中
            (int)XFinApi.TradeApi.ActionKind.Open == notifyParams.ActionType &&
            (int)ResultKind.Success == notifyParams.ResultType
            */

            /* 行情相关方法
            while (!market.IsOpened())
                 Thread.Sleep(1000);

            //订阅行情，已在MarketEvent.OnNotify中订阅
            XFinApi.QueryParams param = new XFinApi.QueryParams();
            param.ExchangeID = Cfg.ExchangeID;
            param.InstrumentID = Cfg.InstrumentID;
            market.Subscribe(param);

            //取消订阅行情
            market.Unsubscribe(param);
            */
        }

        //////////////////////////////////////////////////////////////////////////////////
        //交易测试
        static void TradeTest()
        {
            //创建 ITrade
            // char* path 指 xxx.exe 同级子目录中的 xxx.dll 文件
            int err = -1;

            trade = ITradeApi.XFinApi_CreateTradeApi("XTA_W32/Api/UFXCTP_V3.7.1.10/XFinApi.UFXCTPTradeApi.dll", out err);

            if (err > 0 || trade == null)
            {
                Console.WriteLine(string.Format("* Trade XFinApiCreateError={0};", StrCreateErrors[err]));
                return;
            }

            //注册事件
            tradeEvent = new TradeEvent();
            trade.SetListener(tradeEvent);

            //连接服务器
            OpenParams openParams = new OpenParams();
            openParams.HostAddress = Cfg.TradeAddress;
            openParams.UserID = Cfg.UserName;
            openParams.Password = Cfg.Password;
            openParams.Configs.Add("LicenseFile", Cfg.LicenseFile);
            openParams.Configs.Add("LicensePwd", Cfg.LicensePwd);
            openParams.Configs.Add("SafeLevel", Cfg.SafeLevel);
            openParams.Configs.Add("CertFile", Cfg.CertFile);
            openParams.Configs.Add("CertPwd", Cfg.CertPwd);
            openParams.IsUTF8 = true;
            trade.Open(openParams);

            /*
            //连接成功后才能执行查询、委托等操作，检测方法有两种：
            1、ITrade.IsOpened()=true
            2、TradeListener.OnNotify中
            (int)XFinApi.TradeApi.ActionKind.Open == notifyParams.ActionType &&
			(int)ResultKind.Success == notifyParams.ResultType
             */

            while (!trade.IsOpened())
                Thread.Sleep(1000);

            QueryParams qryParam = new QueryParams();

            //查询委托单
            Thread.Sleep(1000);//有些接口查询有间隔限制，如：CTP查询间隔为1秒
            Console.WriteLine("Press any key to QueryOrder.");
            Console.ReadKey();
            trade.QueryOrder(qryParam);

            //查询成交单
            Thread.Sleep(3000);
            Console.WriteLine("Press any key to QueryTradeOrder.");
            Console.ReadKey();
            trade.QueryTradeOrder(qryParam);

            //查询合约
            qryParam.ExchangeID = Cfg.ExchangeID;
            qryParam.InstrumentID = Cfg.InstrumentID;
            Thread.Sleep(3000);
            Console.WriteLine("Press any key to QueryInstrument.");
            Console.ReadKey();
            trade.QueryInstrument(qryParam);

            //查询持仓
            Thread.Sleep(3000);
            Console.WriteLine("Press any key to QueryPosition.");
            Console.ReadKey();
            trade.QueryPosition(qryParam);

            //查询账户
            Thread.Sleep(3000);
            Console.WriteLine("Press any key to QueryAccount.");
            Console.ReadKey();
            trade.QueryAccount(qryParam);

            //委托下单
            Thread.Sleep(1000);
            Console.WriteLine("Press any key to OrderAction.");
            Console.ReadKey();
            Order order = new Order();
            order.ExchangeID = Cfg.ExchangeID;
            order.InstrumentID = Cfg.InstrumentID;
            order.Price = Cfg.SellPrice1;
            order.Volume = 1;
            order.Direction = DirectionKind.Buy;
            order.OpenCloseType = OpenCloseKind.Open;

            //下单高级选项，可选择性设置
            order.ActionType = OrderActionKind.Insert;//下单
            order.OrderType = OrderKind.Order;//标准单
            order.PriceCond = PriceConditionKind.LimitPrice;//限价
            order.VolumeCond = VolumeConditionKind.AnyVolume;//任意数量
            order.TimeCond = TimeConditionKind.GFD;//当日有效
            order.ContingentCond = ContingentCondKind.Immediately;//立即
            order.HedgeType = HedgeKind.Speculation;//投机
            order.ExecResult = ExecResultKind.NoExec;//没有执行

            trade.OrderAction(order);
        }

        static void Main(string[] args)
        {
            //请先在Config类中设置地址、BrokerID、用户名、密码等信息
            if (Cfg.TradeAddress == "" || Cfg.MarketAddress == "" || Cfg.LicenseFile == "" ||
                Cfg.UserName == "" || Cfg.Password == "")
            {
                Console.WriteLine("请先在Config类中设置地址账户等信息.\n");
                Console.ReadKey();
            }

            MarketTest();
            TradeTest();

            Thread.Sleep(2000);
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();

            //关闭连接
            if (market != null)
            {
                market.Close();
                ITradeApi.XFinApi_ReleaseMarketApi(market);//必须释放资源
            }
            if (trade != null)
            {
                trade.Close();
                ITradeApi.XFinApi_ReleaseTradeApi(trade);//必须释放资源
            }

            Console.WriteLine("Closed.");
            Console.ReadKey();
        }
    }
}

