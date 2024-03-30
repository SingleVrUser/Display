using Newtonsoft.Json;

namespace Display.Models.Api.Aria2;

public class Aria2GlobalOptionRequestResult
{
    [JsonProperty("allowoverwrite")]
    public string AllowOverWrite { get; set; }
    
    [JsonProperty("allowpiecelengthchange")]
    public string AllowPieceLengthChange { get; set; }
    
    [JsonProperty("alwaysresume")]
    public string AlwaysResume { get; set; }
    
    [JsonProperty("asyncdns")]
    public string AsyncDns { get; set; }
    
    [JsonProperty("autofilerenaming")]
    public string AutoFileRenaming { get; set; }
    
    [JsonProperty("autosaveinterval")]
    public string AutoSaveInterval { get; set; }
    
    [JsonProperty("btdetachseedonly")]
    public string BtDetachSeedOnly { get; set; }
    
    [JsonProperty("btenablehookafterhashcheck")]
    public string BtEnableHookAfterHashCheck { get; set; }
    
    [JsonProperty("btenablelpd")]
    public string BtEnableLpd { get; set; }
    
    [JsonProperty("btforceencryption")]
    public string BtForceEncryption { get; set; }
    
    [JsonProperty("bthashcheckseed")]
    public string BtHashCheckSeed { get; set; }
    
    [JsonProperty("btloadsavedmetadata")]
    public string BtLoadSavedMetaData { get; set; }
    
    [JsonProperty("btmaxopenfiles")]
    public string BtMaxOpenFiles { get; set; }
    
    [JsonProperty("btmaxpeers")]
    public string BtMaxPeers { get; set; }
    
    [JsonProperty("btmetadataonly")]
    public string BtMetaDataOnly { get; set; }
    
    [JsonProperty("btmincryptolevel")]
    public string BtMinCryptoLevel { get; set; }
    
    [JsonProperty("btremoveunselectedfile")]
    public string BtRemoveUnselectedFile { get; set; }
    
    [JsonProperty("btrequestpeerspeedlimit")]
    public string BtRequestPeerSpeedLimit { get; set; }
    
    [JsonProperty("btrequirecrypto")]
    public string BtRequireCrypto { get; set; }
    
    [JsonProperty("btsavemetadata")]
    public string BtSaveMetaData { get; set; }
    
    [JsonProperty("btseedunverified")]
    public string BtSeedUnverified { get; set; }
    
    [JsonProperty("btstoptimeout")]
    public string BtStopTimeout { get; set; }
    
    [JsonProperty("bttrackerconnecttimeout")]
    public string BtTrackerConnectTimeout { get; set; }
    
    [JsonProperty("bttrackerinterval")]
    public string BtTrackerInterval { get; set; }
    
    [JsonProperty("bttrackertimeout")]
    public string BtTrackerTimeout { get; set; }
    
    [JsonProperty("cacertificate")]
    public string CaCertificate { get; set; }
    
    [JsonProperty("checkcertificate")]
    public string CheckCertificate { get; set; }
    
    [JsonProperty("checkintegrity")]
    public string CheckIntegrity { get; set; }
    
    [JsonProperty("conditionalget")]
    public string ConditionalGet { get; set; }
    
    [JsonProperty("confpath")]
    public string ConfPath { get; set; }
    
    [JsonProperty("connecttimeout")]
    public string ConnectTimeout { get; set; }
    
    [JsonProperty("consoleloglevel")]
    public string ConsoleLogLevel { get; set; }
    
    [JsonProperty("contentdispositiondefaultutf8")]
    public string ContentDispositionDefaultUtf8 { get; set; }
    
    [JsonProperty("_continue")]
    public string Continue { get; set; }
    
    [JsonProperty("daemon")]
    public string Daemon { get; set; }
    
    [JsonProperty("deferredinput")]
    public string DeferredInput { get; set; }
    
    [JsonProperty("dhtfilepath")]
    public string DhtFilePath { get; set; }
    
    [JsonProperty("dhtfilepath6")]
    public string DhtFilePath6 { get; set; }
    
    [JsonProperty("dhtlistenport")]
    public string DhtListenPort { get; set; }
    
    [JsonProperty("dhtmessagetimeout")]
    public string DhtMessageTimeout { get; set; }
    
    [JsonProperty("dir")]
    public string Dir { get; set; }
    
    [JsonProperty("disableipv6")]
    public string DisableIpv6 { get; set; }
    
    [JsonProperty("diskcache")]
    public string Diskcache { get; set; }
    
    [JsonProperty("downloadresult")]
    public string DownloadResult { get; set; }
    
    [JsonProperty("dryrun")]
    public string DryRun { get; set; }
    
    [JsonProperty("dscp")]
    public string Dscp { get; set; }
    
    [JsonProperty("enablecolor")]
    public string EnableColor { get; set; }
    
    [JsonProperty("enabledht")]
    public string EnableDht { get; set; }
    
    [JsonProperty("enabledht6")]
    public string EnableDht6 { get; set; }
    
    [JsonProperty("enablehttpkeepalive")]
    public string EnableHttpKeepAlive { get; set; }
    
    [JsonProperty("enablehttppipelining")]
    public string EnableHttpPipelining { get; set; }
    
    [JsonProperty("enablemmap")]
    public string EnableMmap { get; set; }
    
    [JsonProperty("enablepeerexchange")]
    public string EnablePeerExchange { get; set; }
    
    [JsonProperty("enablerpc")]
    public string EnableRpc { get; set; }
    
    [JsonProperty("eventpoll")]
    public string EventPoll { get; set; }
    
    [JsonProperty("fileallocation")]
    public string FileAllocation { get; set; }
    
    [JsonProperty("followmetalink")]
    public string FollowMetaLink { get; set; }
    
    [JsonProperty("followtorrent")]
    public string FollowTorrent { get; set; }
    
    [JsonProperty("forcesave")]
    public string ForceSave { get; set; }
    
    [JsonProperty("ftppasv")]
    public string FtpPasv { get; set; }
    
    [JsonProperty("ftpreuseconnection")]
    public string FtpReuseConnection { get; set; }
    
    [JsonProperty("ftptype")]
    public string FtpType { get; set; }
    
    [JsonProperty("hashcheckonly")]
    public string HashCheckOnly { get; set; }
    
    [JsonProperty("help")]
    public string Help { get; set; }
    
    [JsonProperty("httpacceptgzip")]
    public string HttpAcceptGzip { get; set; }
    
    [JsonProperty("httpauthchallenge")]
    public string HttpAuthChallenge { get; set; }
    
    [JsonProperty("httpnocache")]
    public string HttpNoCache { get; set; }
    
    [JsonProperty("humanreadable")]
    public string HumanReadable { get; set; }
    
    [JsonProperty("keepunfinisheddownloadresult")]
    public string KeepUnfinishedDownloadResult { get; set; }
    
    [JsonProperty("listenport")]
    public string ListenPort { get; set; }
    
    [JsonProperty("loglevel")]
    public string Loglevel { get; set; }
    
    [JsonProperty("lowestspeedlimit")]
    public string LowestSpeedLimit { get; set; }
    
    [JsonProperty("maxconcurrentdownloads")]
    public string MaxConcurrentDownloads { get; set; }
    
    [JsonProperty("maxconnectionperserver")]
    public string MaxConnectionPerServer { get; set; }
    
    [JsonProperty("maxdownloadlimit")]
    public string MaxDownloadLimit { get; set; }
    
    [JsonProperty("maxdownloadresult")]
    public string MaxDownloadResult { get; set; }
    
    [JsonProperty("maxfilenotfound")]
    public string MaxFileNotFound { get; set; }
    
    [JsonProperty("maxmmaplimit")]
    public string MaxMmapLimit { get; set; }
    
    [JsonProperty("maxoveralldownloadlimit")]
    public string MaxOverallDownloadLimit { get; set; }
    
    [JsonProperty("maxoveralluploadlimit")]
    public string MaxOverallUploadLimit { get; set; }
    
    [JsonProperty("maxresumefailuretries")]
    public string MaxResumeFailureTries { get; set; }
    
    [JsonProperty("maxtries")]
    public string MaxTries { get; set; }
    
    [JsonProperty("maxuploadlimit")]
    public string MaxUploadLimit { get; set; }
    
    [JsonProperty("metalinkenableuniqueprotocol")]
    public string MetaLinkEnableUniqueProtocol { get; set; }
    
    [JsonProperty("metalinkpreferredprotocol")]
    public string MetaLinkPreferredProtocol { get; set; }
    
    [JsonProperty("minsplitsize")]
    public string MinSplitSize { get; set; }
    
    [JsonProperty("mintlsversion")]
    public string MinTlsVersion { get; set; }
    
    [JsonProperty("netrcpath")]
    public string NetrcPath { get; set; }
    
    [JsonProperty("noconf")]
    public string NoConf { get; set; }
    
    [JsonProperty("nofileallocationlimit")]
    public string NoFileAllocationLimit { get; set; }
    
    [JsonProperty("nonetrc")]
    public string NoNetrc { get; set; }
    
    [JsonProperty("optimizeconcurrentdownloads")]
    public string OptimizeConcurrentDownloads { get; set; }
    
    [JsonProperty("parameterizeduri")]
    public string ParameterizedUri { get; set; }
    
    [JsonProperty("pausemetadata")]
    public string PauseMetadata { get; set; }
    
    [JsonProperty("peeragent")]
    public string PeerAgent { get; set; }
    
    [JsonProperty("peeridprefix")]
    public string PeerIdPrefix { get; set; }
    
    [JsonProperty("piecelength")]
    public string PieceLength { get; set; }
    
    [JsonProperty("proxymethod")]
    public string ProxyMethod { get; set; }
    
    [JsonProperty("quiet")]
    public string Quiet { get; set; }
    
    [JsonProperty("realtimechunkchecksum")]
    public string RealTimeChunkCheckSum { get; set; }
    
    [JsonProperty("remotetime")]
    public string RemoteTime { get; set; }
    
    [JsonProperty("removecontrolfile")]
    public string RemoveControlFile { get; set; }
    
    [JsonProperty("retrywait")]
    public string RetryWait { get; set; }
    
    [JsonProperty("reuseuri")]
    public string ReuseUri { get; set; }
    
    [JsonProperty("rlimitnofile")]
    public string RLimitNoFile { get; set; }
    
    [JsonProperty("rpcalloworiginall")]
    public string RpcAllowOriginAll { get; set; }
    
    [JsonProperty("rpclistenall")]
    public string RpcListenAll { get; set; }
    
    [JsonProperty("rpclistenport")]
    public string RpcListenPort { get; set; }
    
    [JsonProperty("rpcmaxrequestsize")]
    public string RpcMaxRequestSize { get; set; }
    
    [JsonProperty("rpcsaveuploadmetadata")]
    public string RpcSaveUploadMetadata { get; set; }
    
    [JsonProperty("rpcsecure")]
    public string RpcSecure { get; set; }
    
    [JsonProperty("savenotfound")]
    public string SaveNotFound { get; set; }
    
    [JsonProperty("savesession")]
    public string SaveSession { get; set; }
    
    [JsonProperty("savesessioninterval")]
    public string SaveSessionInterval { get; set; }
    
    [JsonProperty("seedratio")]
    public string SeedRatio { get; set; }
    
    [JsonProperty("serverstattimeout")]
    public string ServerStatTimeout { get; set; }
    
    [JsonProperty("showconsolereadout")]
    public string ShowConsoleReadOut { get; set; }
    
    [JsonProperty("showfiles")]
    public string ShowFiles { get; set; }
    
    [JsonProperty("socketrecvbuffersize")]
    public string SocketRecvBufferSize { get; set; }
    
    [JsonProperty("split")]
    public string Split { get; set; }
    
    [JsonProperty("stderr")]
    public string Stderr { get; set; }
    
    [JsonProperty("stop")]
    public string Stop { get; set; }
    
    [JsonProperty("streampieceselector")]
    public string StreamPieceSelector { get; set; }
    
    [JsonProperty("summaryinterval")]
    public string SummaryInterval { get; set; }
    
    [JsonProperty("timeout")]
    public string Timeout { get; set; }
    
    [JsonProperty("truncateconsolereadout")]
    public string TruncateConsoleReadOut { get; set; }
    
    [JsonProperty("uriselector")]
    public string UriSelector { get; set; }
    
    [JsonProperty("usehead")]
    public string UseHead { get; set; }
    
    [JsonProperty("useragent")]
    public string Useragent { get; set; }
}