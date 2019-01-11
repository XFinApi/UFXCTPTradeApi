######################################################
# UFXCTPTradeApiSimpleDemoPythonPyCharmC2018
# www.xfinapi.com
######################################################
import XFinApi_TradeApi
import time


# Python版本说明：Api使用32位python-3.6版本编译，建议开发时也使用该版本。#


# 配置信息
class Config:
    # 注册UFXCTP模拟账户:QQ群586525357

    # 地址账户
    MarketAddress = "";
    TradeAddress = "";
    UserName = "";  # 公用测试账户。为了测试准确，请注册使用您自己的账户。
    Password = "";
    LicenseFile = "license.dat";
    LicensePwd = "";
    SafeLevel = "";
    CertFile = "";
    CertPwd = "";

    # 合约
    ExchangeID = "1";  # 上证:1, 深证:2, 郑商:F1, 大商:F2, 上期:F3, 中金:F4, 能源中心:F5
    InstrumentID = "10001505";

    # 行情
    AskPrice1 = -1;
    BidPrice1 = -1;


# API
# 创建失败错误码的含义，其他错误码的含义参见XTA_W32\Cpp\ApiEnum.h文件

StrCreateErrors = [
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
    "认证超时"]


class MarketEvent(XFinApi_TradeApi.MarketListener):
    cfg = 0
    market = 0

    def __init__(self,market,cfg):
        XFinApi_TradeApi.MarketListener.__init__(self)
        self.market = market
        self.cfg = cfg

    def OnNotify(self, notifyParams: 'NotifyParams'):
        print("* Market")
        str = ""
        for codeinfo in notifyParams.CodeInfos:
            str += "(Code={};LowerCode={};LowerMessage={})".format(
                codeinfo.Code, codeinfo.LowerCode, codeinfo.LowerMessage)
        print(" OnNotify: Action={}, Result={}{}".format(
            notifyParams.ActionType,
            notifyParams.ResultType,
            str
        ))
        if XFinApi_TradeApi.ActionKind_Open == notifyParams.ActionType and XFinApi_TradeApi.ResultKind_Success == notifyParams.ResultType:
            # 订阅
            param = XFinApi_TradeApi.QueryParams()
            param.ExchangeID = self.cfg.ExchangeID
            param.InstrumentID = self.cfg.InstrumentID
            self.market.Subscribe(param)

    def OnSubscribed(self,instInfo):
        print("- OnSubscribed:{}".format(instInfo.InstrumentID))

    def OnUnsubscribed(self,instInfo):
        print("- OnUnsubscribed:{}".format(instInfo.InstrumentID))

    def OnTick(self,tick):
        if self.cfg.AskPrice1 <= 0 and self.cfg.BidPrice1 <= 0:
            print(" Tick,{}, HighestPrice={}, LowestPrice={}, BidPrice0={}, BidVolume0={}, AskPrice0={}, AskVolume0={}, LastPrice={}, TotalVolume={}, TradingTime={}".format(
                tick.InstrumentID,
                tick.HighestPrice,
                tick.LowestPrice,
                tick.GetBidPrice(0),
                tick.GetBidVolume(0),
                tick.GetAskPrice(0),
                tick.GetAskVolume(0),
                tick.LastPrice,
                tick.TotalVolume,
                tick.TradingTime))
        self.cfg.AskPrice1 = tick.GetAskPrice(0)
        self.cfg.BidPrice1 = tick.GetBidPrice(0)


class MarketTest:
    market = 0
    cfg = 0
    marketEvent = 0

    def __init__(self,cfg):
        self.cfg = cfg

    def __del__(self):
        #if isinstance(self.marketEvent,MarketEvent):
            #del self.marketEvent
        if isinstance(self.market,XFinApi_TradeApi.IMarket):
            XFinApi_TradeApi.XFinApi_ReleaseMarketApi(self.market)

    def test(self):
        self.market = XFinApi_TradeApi.XFinApi_CreateMarketApi("XTA_W32/Api/UFXCTP_V3.7.1.10/XFinApi.UFXCTPTradeApi.dll")
        if isinstance(self.market,int):
            print("* Market XFinApiCreateError={};".format(StrCreateErrors[self.market]))
            return
        else:
            self.market = self.market[0]
        self.marketEvent = MarketEvent(self.market,self.cfg)
        self.market.SetListener(self.marketEvent)
        openParams = XFinApi_TradeApi.OpenParams()
        openParams.HostAddress = self.cfg.MarketAddress
        openParams.UserID = self.cfg.UserName
        openParams.Password = self.cfg.Password
        openParams.Configs["LicenseFile"] = self.cfg.LicenseFile
        openParams.IsUTF8 = True
        self.market.Open(openParams)


class TradeEvent(XFinApi_TradeApi.TradeListener):
    cfg = 0
    trade = 0

    def __init__(self,trade,cfg):
        XFinApi_TradeApi.TradeListener.__init__(self)
        self.trade = trade
        self.cfg = cfg

    def OnNotify(selfs,notifyParams):
        print("* Trade")
        str = ""
        for codeinfo in notifyParams.CodeInfos:
            str += "(Code={};LowerCode={};LowerMessage={})".format(
                codeinfo.Code, codeinfo.LowerCode, codeinfo.LowerMessage)
        print(" OnNotify: Action={}, Result={}{}".format(
            notifyParams.ActionType,
            notifyParams.ResultType,
            str
        ))

    def OnUpdateOrder(self,order):
        print("- OnUpdateOrder:")
        print("Ref={}, ID={}, InstID={}, Price={}, Volume={}, NoTradedVolume={}, Direction={}, "
              "PriceCond={}, SessionID={}, Status={}, Msg={}, {}".format(
            order.OrderRef, order.OrderID,
            order.InstrumentID, order.Price, order.Volume, order.NoTradedVolume,
            order.Direction, order.PriceCond, order.SessionID,
            order.Status, order.StatusMsg, order.OrderTime))

    def OnUpdateTradeOrder(self,trade):
        print("- OnUpdateTradeOrder:")
        print(" ID={}, OrderRef={}, InstID={}, Price={}, Volume={}, Direction={}, {}".format(
            trade.TradeID, trade.OrderID, trade.InstrumentID, trade.Price, trade.Volume,
            trade.Direction, trade.TradeTime))

    def OnQueryOrder(self,orders):
        print("- OnQueryOrder:")
        for order in orders:
            print("Ref={}, ID={}, InstID={}, Price={}, Volume={}, NoTradedVolume={}, Direction={}, "
                  "PriceCond={}, SessionID={}, Status={}, Msg={}, {}".format(
                order.OrderRef, order.OrderID,
                order.InstrumentID, order.Price, order.Volume, order.NoTradedVolume,
                order.Direction, order.PriceCond, order.SessionID,
                order.Status, order.StatusMsg, order.OrderTime))

    def OnQueryTradeOrder(self,trades):
        print("- OnQueryTradeOrder:")
        for trade in trades:
            print(" ID={}, OrderRef={}, InstID={}, Price={}, Volume={}, Direction={}, {}".format(
                trade.TradeID, trade.OrderID, trade.InstrumentID, trade.Price, trade.Volume,
                trade.Direction, trade.TradeTime))

    def OnQueryInstrument(self,instruments):
        print("- OnQueryInstrument:")
        for inst in instruments:
            print("ExchangeID={}, ProductID={}, ID={}, Name={}".format(inst.ExchangeID, inst.ProductID,inst.InstrumentID, inst.InstrumentName))

    def OnQueryPosition(self,poss):
        print("- OnQueryPosition:")
        for pos in poss:
            positionTotal = 0
            posSellable = 0
            posYestoday = 0
            avgPrice = 0
            if XFinApi_TradeApi.IsDefaultValue(pos.PositionTotal) == False:
                positionTotal = pos.PositionTotal
            if XFinApi_TradeApi.IsDefaultValue(pos.PositionSellable) == False:
                posSellable = pos.PositionSellable
            if XFinApi_TradeApi.IsDefaultValue(pos.PositionYesterday) == False:
                posYestoday = pos.PositionYesterday
            if XFinApi_TradeApi.IsDefaultValue(pos.AvgPrice) == False:
                avgPrice = pos.AvgPrice
            print(" InstID={}, Direction={}, PositionTotal={}, PositionSellable={}, PositionYesterday={}, AvgPrice={}".format(
                pos.InstrumentID, pos.Direction, positionTotal, posSellable, posYestoday, avgPrice))

    def OnQueryAccount(self,accInfo):
         print("- OnQueryAccount:")
         print(" Balance={}, Available={}, CloseProfit={}, Commission={}, PositionProfit={}, CurrMargin={}".format(
             accInfo.Balance, accInfo.Available, accInfo.CloseProfit, accInfo.Commission, accInfo.PositionProfit, accInfo.CurrMargin))


class TradeTest:
    cfg = 0
    trade = 0
    tradeEvent = 0

    def __init__(self,cfg):
        self.cfg = cfg

    def __del__(self):
        if isinstance(self.tradeEvent,TradeEvent):
            del self.tradeEvent
        if isinstance(self.trade,XFinApi_TradeApi.ITrade):
            XFinApi_TradeApi.XFinApi_ReleaseTradeApi(self.trade)

    def Test(self):
        # 创建ITrade char * path指xxx.exe同级子目录中的xxx.dll文件
        self.trade = XFinApi_TradeApi.XFinApi_CreateTradeApi(
            "XTA_W32/Api/UFXCTP_V3.7.1.10/XFinApi.UFXCTPTradeApi.dll")
        if isinstance(self.trade,int):
            print("* Trade XFinApiCreateError={};".format(StrCreateErrors[self.trade]))
            return
        else:
            self.trade = self.trade[0]

        self.tradeEvent = TradeEvent(self.trade,self.cfg)
        self.trade.SetListener(self.tradeEvent)
        openParams = XFinApi_TradeApi.OpenParams()
        openParams.HostAddress = self.cfg.TradeAddress
        openParams.UserID = self.cfg.UserName
        openParams.Password = self.cfg.Password
        openParams.Configs["LicenseFile"] = self.cfg.LicenseFile
        openParams.Configs["LicensePwd"] = self.cfg.LicensePwd
        openParams.Configs["SafeLevel"] = self.cfg.SafeLevel
        openParams.Configs["CertFile"] = self.cfg.CertFile
        openParams.Configs["CertPwd"] = self.cfg.CertPwd
        openParams.IsUTF8 = True

        self.trade.Open(openParams)

        # 连接成功后才能执行查询、委托等操作，检测方法有两种：
        # 1、self.trade.IsOpened() == 1
        # 2、TradeEvent的OnNotify中
        #  XFinApi_TradeApi.ActionKind_Open == notifyParams.ActionType and XFinApi_TradeApi.ResultKind_Success == notifyParams.ResultType
        while self.trade.IsOpened() != 1:
            time.sleep(1)

        qryParam = XFinApi_TradeApi.QueryParams()

        # 查询委托单 有些接口查询有间隔限制，如：CTP查询间隔为1秒
        time.sleep(1)
        print("Press any key to QueryOrder.")
        input()
        self.trade.QueryOrder(qryParam)

        # 查询成交单
        time.sleep(3)
        print("Press any key to QueryTradeOrder.")
        input()
        self.trade.QueryTradeOrder(qryParam)

        # 查询合约
        qryParam.ExchangeID = self.cfg.ExchangeID
        qryParam.InstrumentID = self.cfg.InstrumentID
        time.sleep(3)
        print("Press any key to QueryInstrument.")
        input()
        self.trade.QueryInstrument(qryParam)

        # 查询持仓
        time.sleep(3)
        print("Press any key to QueryPosition.")
        input()
        self.trade.QueryPosition(qryParam)

        # 查询账户
        time.sleep(3)
        print("Press any key to QueryAccount.")
        input()
        self.trade.QueryAccount(qryParam)

        # 委托下单
        time.sleep(1)
        print("Press any key to OrderAction.");
        input()
        order = XFinApi_TradeApi.Order()
        order.ExchangeID = self.cfg.ExchangeID;
        order.InstrumentID = self.cfg.InstrumentID
        order.Price = self.cfg.AskPrice1
        order.Volume = 1
        order.Direction = XFinApi_TradeApi.DirectionKind_Buy
        order.OpenCloseType = XFinApi_TradeApi.OpenCloseKind_Open

        # 下单高级选项，可选择性设置
        order.ActionType = XFinApi_TradeApi.OrderActionKind_Insert  # 下单
        order.OrderType = XFinApi_TradeApi.OrderKind_Order  # 标准单
        order.PriceCond = XFinApi_TradeApi.PriceConditionKind_LimitPrice  # 限价
        order.VolumeCond = XFinApi_TradeApi.VolumeConditionKind_AnyVolume  # 任意数量
        order.TimeCond = XFinApi_TradeApi.TimeConditionKind_GFD  # 当日有效
        order.ContingentCond = XFinApi_TradeApi.ContingentCondKind_Immediately  # 立即
        order.HedgeType = XFinApi_TradeApi.HedgeKind_Speculation  # 投机

        self.trade.OrderAction(order)


def main():
    cfg = Config

    # 请先在Config类中设置地址、BrokerID、用户名、密码等信息
    if cfg.TradeAddress == "" or cfg.MarketAddress == "" or cfg.LicenseFile == "" or cfg.UserName == "" or cfg.Password == "" :
        print("请先在Config类中设置地址账户等信息.\n")
        input()

    mt = MarketTest(cfg)
    mt.test()
    tt = TradeTest(cfg)
    tt.Test()
    time.sleep(10)
    del mt
    del tt


if __name__ == "__main__":
    main()
