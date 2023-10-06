using ETS.Core.Api;
using ETS.Core.Api.Models;
using ETS.Core.Api.Models.Data;
using ETS.Core.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using WCS.ConveyorProcessLogic;

namespace WCS
{
  public class ConveyorProceesLogic
  {
    public bool ScanRunning
    {
      get; set;
    }

    public bool Loaded
    {
      get; set;
    }

    readonly bool debug;
    const string logCategoryBase = "WCS.ConveyorProceesLogic";
    private string logCategory;
    DbSystemType systemType;
    DbSystem system = new DbSystem();
    RFIDProductIdentificationConveyor RFIDProductIdentificationProcessLogic;
    RoutingConveyor RoutingProcessLogic;
    PassThroughConveyor PassThroughProcessLogic;
    BufferConveyor BufferProcessLogic;
    GroupingConveyor GroupingProcessLogic;
    GroupBufferConveyor GroupBufferProcessLogic;
    StackerConveyor StackerProcessLogic;
    PalletizerConveyor PalletizerProcessLogic;
    PrintAndLabelingConveyor PrintAndLabelingProcessLogic;
    RFIDPalletIdentificationConveyor RFIDPalletIdentificationProcessLogic;
    HooderConveyor HooderConveyorProcessLogic;
    DropoffPickupConveyor DropoffPickupProcessLogic;
    ManualWorkstationConveyor ManualWorkstationProcessLogic;
    PalletReintroductionConveyor PalletReintroductionProcessLogic;
    UnitConsumptionConveyor UnitConsumptionProcessLogic;
    RFIDDiverterConveyor RFIDDiverterProcessLogic;

    public ConveyorProceesLogic(int _systemID, string _server, string _databaseName, string _login, string _password, bool _debug = false)
    {
      debug = _debug;
      ApiService api;
      ConnectionInfo ci = new ConnectionInfo();
      try
      {
        ci.Server = _server;
        ci.Name = _databaseName;
        ci.Login = _login;
        ci.Password = _password;
        ci.UsesWindowsAuthentication = false;
        api = ApiService.StartupNewApplicationWithConnectionSettings(ci);
        Loaded = LoadAndInitialize(_systemID, api);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Api Connection");
      }
    }

    public ConveyorProceesLogic(int _systemID, bool _debugMode)
    {
      try
      {
        ApiService api;
        api = ApiService.GetInstance().ThrowIfNull("Failed to Connect to the TackSYS api");
        debug = _debugMode;
        Loaded = LoadAndInitialize(_systemID, api);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Api Connection");
      }
    }

    private bool LoadAndInitialize(int systemID, ApiService api)
    {
      if (api == null)
      {
        Console.WriteLine("Failed to Connect to the TackSYS api");
        return false;
      }
      try
      {        
        system = api.Data.DbSystem.Load.ByID(systemID);
        logCategory = logCategoryBase + "(" + system.ID.ToString() + ")";
        if (system == null) return api.Util.Log.WriteError("Unable to load System object for ID {0}.".FormatWith(systemID), "SystemScript");
        systemType = api.Data.DbSystemType.Load.ByID(system.SystemTypeID);
        switch (systemType.Key)
        {
          case "ST.CONV.RFIDREADERLINKED":
            RFIDProductIdentificationProcessLogic = new RFIDProductIdentificationConveyor(api, systemID, debug);
            break;
          case "ST.CONV.TURNTABLE":
            RoutingProcessLogic = new RoutingConveyor(api, systemID, debug);
            break;
          case "ST.CONV.PASSTHROUGH":
            PassThroughProcessLogic = new PassThroughConveyor(api, systemID, debug);
            break;
          case "ST.CONV.BUFFER":
            BufferProcessLogic = new BufferConveyor(api, systemID, debug);
            break;
          case "ST.CONV.GROUPING":
            GroupingProcessLogic = new GroupingConveyor(api, systemID, debug);
            break;
          case "ST.CONV.GROUPBUFFER":
            GroupBufferProcessLogic = new GroupBufferConveyor(api, systemID, debug);
            break;
          case "ST.CONV.STACKERSYSTEM":
            StackerProcessLogic = new StackerConveyor(api, systemID, debug);
            break;
          case "ST.CONV.PRINTANDLABELINGSTATION":
            PrintAndLabelingProcessLogic = new PrintAndLabelingConveyor(api, systemID, debug);
            break;
          case "ST.CONV.RFIDREADERSYSTEM":
            RFIDPalletIdentificationProcessLogic = new RFIDPalletIdentificationConveyor(api, systemID, debug);
            break;
          case "ST.CONV.PALLETIZER":
            PalletizerProcessLogic = new PalletizerConveyor(api, systemID, debug);
            break;
          case "ST.CONV.HOODERSTATION":
            HooderConveyorProcessLogic = new HooderConveyor(api, systemID, debug);
            break;
          case "ST.CONV.DROPOFFPICKUPPOINT":
            DropoffPickupProcessLogic = new DropoffPickupConveyor(api, systemID, debug);
            break;
          case "ST.CONV.MANUALWORKSTATION":
            ManualWorkstationProcessLogic = new ManualWorkstationConveyor(api, systemID, debug);
            break;
          case "ST.CONV.PALLETREINTRODUCTIONSTATION":
            PalletReintroductionProcessLogic = new PalletReintroductionConveyor(api, systemID, debug);
            break;
          case "ST.CONV.UNITCONSUMPTION":
            UnitConsumptionProcessLogic = new UnitConsumptionConveyor(api, systemID, debug);
            break;
          case "ST.CONV.DIVERTERRFID":
            RFIDDiverterProcessLogic = new RFIDDiverterConveyor(api, systemID, debug);
            break;

        }
      }
      catch (Exception ex)
      {
        api.Util.Log.WriteError("LoadAndInitialize(): {0}. @ {1}".FormatWith(ex.Message, ex.StackTrace), logCategory);
      }
      return true;
    }

    public async Task Execute()
    {
      if (Loaded)
      {
        ScanRunning = true;
        switch (systemType.Key)
        {
          case "ST.CONV.RFIDREADERLINKED":
            RFIDProductIdentificationProcessLogic.CheckForReplies();
            RFIDProductIdentificationProcessLogic.MainScript();
            break;
          case "ST.CONV.TURNTABLE":
            RoutingProcessLogic.CheckForReplies();
            RoutingProcessLogic.MainScript();
            break;
          case "ST.CONV.PASSTHROUGH":
            PassThroughProcessLogic.CheckForReplies();
            PassThroughProcessLogic.MainScript();
            break;
          case "ST.CONV.BUFFER":
            BufferProcessLogic.CheckForReplies();
            BufferProcessLogic.MainScript();
            break;
          case "ST.CONV.GROUPING":
            GroupingProcessLogic.CheckForReplies();
            GroupingProcessLogic.MainScript();
            break;
          case "ST.CONV.GROUPBUFFER":
            GroupBufferProcessLogic.CheckForReplies();
            GroupBufferProcessLogic.MainScript();
            GroupBufferProcessLogic.CheckToRelease();
            break;
          case "ST.CONV.STACKERSYSTEM":
            StackerProcessLogic.CheckForReplies();
            StackerProcessLogic.MainScript();
            break;
          case "ST.CONV.PRINTANDLABELINGSTATION":
            PrintAndLabelingProcessLogic.CheckForReplies();
            PrintAndLabelingProcessLogic.MainScript();
            break;
          case "ST.CONV.RFIDREADERSYSTEM":
            RFIDPalletIdentificationProcessLogic.CheckForReplies();
            RFIDPalletIdentificationProcessLogic.MainScript();
            break;
          case "ST.CONV.PALLETIZER":
            PalletizerProcessLogic.CheckForReplies();
            PalletizerProcessLogic.MainScript();
            break;
          case "ST.CONV.HOODERSTATION":
            HooderConveyorProcessLogic.CheckForReplies();
            HooderConveyorProcessLogic.MainScript();
            break;
          case "ST.CONV.DROPOFFPICKUPPOINT":
            DropoffPickupProcessLogic.CheckForReplies();
            DropoffPickupProcessLogic.MainScript();
            break;
          case "ST.CONV.MANUALWORKSTATION":
            ManualWorkstationProcessLogic.CheckForReplies();
            ManualWorkstationProcessLogic.MainScript();
            break;
          case "ST.CONV.PALLETREINTRODUCTIONSTATION":
            PalletReintroductionProcessLogic.CheckForReplies();
            PalletReintroductionProcessLogic.MainScript();
            break;
          case "ST.CONV.UNITCONSUMPTION":
            UnitConsumptionProcessLogic.CheckForReplies();
            UnitConsumptionProcessLogic.MainScript();
            break;
          case "ST.CONV.DIVERTERRFID":
            RFIDDiverterProcessLogic.CheckForReplies();
            RFIDDiverterProcessLogic.MainScript();
            break;
        }
        ScanRunning = false;
      }
    }
  }
}
