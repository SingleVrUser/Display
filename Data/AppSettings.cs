using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Data
{
    public class AppSettings
    {
        public static ApplicationDataContainer localSettings { get { return ApplicationData.Current.LocalSettings; } }

        /// <summary>
        /// 115导入数据库 进程界面 的 任务完成后通知
        /// </summary>
        public static bool ProgressOfImportDataAccess_IsToastAfterTask;

        /// <summary>
        /// 115的Cookie
        /// </summary>
        public static string _115_Cookie
        {
            get
            {
                return localSettings.Values["Cookie"] as string;
            }
            set
            {
                localSettings.Values["Cookie"] = value;
            }
        }

        /// <summary>
        /// JavBus网址
        /// </summary>
        /// 
        private static string _javBus_BaseUrl = "https://www.javbus.com/";
        public static string JavBus_BaseUrl
        {
            get
            {
                var localJavBusBaseUrl = localSettings.Values["JavBus_BaseUrl"] as string;
                if (string.IsNullOrEmpty(localJavBusBaseUrl))
                {
                    localJavBusBaseUrl = _javBus_BaseUrl;
                }
                return localJavBusBaseUrl;
            }
            set
            {
                localSettings.Values["JavBus_BaseUrl"] = value;
            }
        }

        /// <summary>
        /// JavDB网址
        /// </summary>
        private static string _javDB_BaseUrl = "https://javdb.com/";
        public static string JavDB_BaseUrl
        {
            get
            {
                var localJavDBBaseUrl = localSettings.Values["JavDB_BaseUrl"] as string;
                if (string.IsNullOrEmpty(localJavDBBaseUrl))
                {
                    localJavDBBaseUrl = _javDB_BaseUrl;
                }
                return localJavDBBaseUrl;
            }
            set
            {
                localSettings.Values["JavDB_BaseUrl"] = value;
            }
        }

        /// <summary>
        /// JavDB的Cookie，查询FC信息需要
        /// </summary>
        public static string javdb_Cookie
        {
            get
            {
                return localSettings.Values["javDB_Cookie"] as string;
            }
            set
            {
                localSettings.Values["javDB_Cookie"] = value;
            }
        }

        /// <summary>
        /// 图片保存地址
        /// </summary>
        public static string Image_SavePath
        {
            get
            {
                string savePath = localSettings.Values["ImageSave_Path"] as string;
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path,"Image");
                    FileMatch.CreateDirectoryIfNotExists(savePath);
                }
                return savePath;
            }
            set
            {
                localSettings.Values["ImageSave_Path"] = value;
            }
        }

        /// <summary>
        /// 图片保存地址
        /// </summary>
        public static string ActorInfo_SavePath
        {
            get
            {
                string savePath = localSettings.Values["ActorInfo_SavePath"] as string;
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Actor");
                    FileMatch.CreateDirectoryIfNotExists(savePath);
                }
                return savePath;
            }
            set
            {
                localSettings.Values["ActorInfo_SavePath"] = value;
            }
        }

        /// <summary>
        /// 演员头像仓库文件保存地址
        /// </summary>
        public static string ActorFileTree_SavePath
        {
            get
            {
                string savePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Data");
                if (!Directory.Exists(savePath))
                {
                    FileMatch.CreateDirectoryIfNotExists(savePath);
                }
                string filePath = Path.Combine(savePath, "Filetree.json");

                return filePath;
            }
        }

        /// <summary>
        /// 数据文件存储地址
        /// </summary>
        public static string DataAccess_SavePath
        {
            get
            {
                string savePath = localSettings.Values["DataAccess_SavePath"] as string;
                if (string.IsNullOrEmpty(savePath))
                {
                    savePath = ApplicationData.Current.LocalFolder.Path;
                }
                return savePath;
            }
            set
            {
                localSettings.Values["DataAccess_SavePath"] = value;
            }
        }

        private static string _matchVideoKeywordsString = "aarm,abp,abs,abw,acme,adn,adz,ajvr,aka,ama,anzd,aoz,apaa,apak,apar,apkh,apns,ara,area,atfb,atid,averv,avkh,avop,avsa,bagr,bahp,baze,bazx,bbi,bcv,bda,beb,bgn,bijn,bikmvr,blor,bmvr,bnst,booty,bst,bstc,bta,btha,buena,caca,cafr,cami,cand,capi,cawd,cbx,cead,cemd,cesd,cetd,chn,chsd,cjod,club,cmd,crnx,crs,crvr,csdx,cspl,cute,cwm,cwpbd,czbd,damz,dandy,dasd,dcv,ddff,ddhh,ddk,ddkh,ddob,ddt,dfe,dgcemd,dgcesd,dgl,diam,dic,dioguitar,dkn,dldss,dmdg,dnin,dnw,docp,doks,dpmi,dpmx,dpsdl,drc,dsambd,dss,dsvr,dtt,dtvr,dtw,dvaj,dvdes,dvsr,ebod,ebvr,ecb,ecr,edd,eiki,ekdv,ekdv,emoi,endx,erofv,erov,esk,etn,etqr,eva,evo,eyan,ezd,fai,fanh,fch,fcp,fdd,fffs,finh,flns,fneo,fsdss,fset,fwdv,gachi,gachig,gachip,gachippv,gana,gaor,garea,gav,gcf,gdhh,geki,genm,gent,gnab,gnax,gns,good,gopj,gs,gvg,gvh,gwaz,gzap,haru,hbad,hey,hgot,hhh,hiz,hmgl,hmn,hnd,hnky,hnvr,hodv,hoi,hoiz,homa,homev,honb,hrv,hunt,hunta,huntb,hunvr,hxad,hxae,hxak,hypn,hzgd,ibw,idbd,ienf,iesp,ing,instv,inu,ion,ipit,iptd,ipvr,ipx,ipz,jac,jag,jan,japornxxx,jbs,jfyg,job,johs,josi,jpsvr,jrai,juc,jufd,jufe,jukf,jul,jusd,juvr,jux,juy,jvr,kamef,kane,kavr,kawd,kbi,kimu,kiray,kird,kire,kiwvr,kmhr,kmhrs,kmvr,knam,knb,kncs,kray,krnd,ksbe,ktb,ktds,ktkp,ktkz,kuro,kuse,kvr,kwp,kyun,lafbd,las,lcbd,lcdv,llr,lol,loli,love,lpbr,lulu,luxu,lxvs,lzdm,maan,madm,man,mao,maraa,mas,maxvr,maxvrh,mcb,mcbd,mcdv,mct,mdb,mdbk,mdm,mdtm,mdvr,meyd,mfc,mfcs,mgt,miaa,miad,miae,midd,mide,midv,mifd,migd,mild,milk,mimk,mint,mird,mism,mist,mium,mix,mizd,mkck,mkmp,mkon,mmb,mmgh,mmks,mmnt,mmus,mogi,mopg,mrmm,msd,msfh,mstd,mste,mstt,mtall,mtm,mudr,mukc,mum,mvsd,mxbd,mxgs,myhd,mywife,nacr,nade,naka,nama,natr,nfdm,ngod,nhdta,nhdtb,nhvr,nitr,nmc,nnpj,nps,nsfs,nsps,nsstl,ntk,ntr,ntrd,nttr,nzk,oae,odfm,ofje,oigs,okad,okb,okp,onez,ongbak,oni,onsd,ore,orebms,orec,oretd,orex,oyc,pb,pbd,per,pgd,pgm,pgod,pipivr,piyo,pkpd,ppft,pppd,pppe,ppt,ppvr,pred,prtd,prvr,psd,psz,pxh,pxvr,qqcm,qrvr,raw,rbd,real,rebd,rebdb,red,rhts,rki,rmd,rmds,roe,room,royd,rpin,rtp,s2mbd,saba,sad,sama,same,savr,sbmx,scp,sdab,sdam,sdde,sdjs,sdmf,sdmm,sdms,sdmt,sdmu,sdnm,sdnt,sdsi,senn,sero,sflb,sga,sgk,sgla,shkd,shl,shn,shpv,shs,shyn,silk,sim,simm,siro,siro,sis,siv,sivr,skmj,sky,skyhd,smbd,smd,snis,soe,spc,spivr,spro,sps,sqte,sqtevr,srho,srs,srtd,ssis,ssni,sspd,star,stars,std,stko,stsk,suke,supa,supd,svdvd,sw,sweet,sxar,tar,tbl,tek,ten,tgav,tigr,tikp,tki,tkwa,tmavr,tmvi,tnb,top,tppn,tpvr,tre,trg,tsf,tus,tyod,ueh,ult,upsm,ure,urvk,urvrsp,usba,val,vdd,vec,vema,venu,venx,vgd,voss,vov,vovs,vrkm,vrtb,vrtm,vrvr,vspds,wa,waaa,wanz,wat,wavr,wbdv,wdi,wf,wnz,wps,wpvr,xrw,xss,xvsr,yab,ymdd,ymds,yrh,yrll,yrz,zmen,zsd,zuko";
        public static string MatchVideoKeywordsString
        {
            get
            {
                string localStr = localSettings.Values["MatchVideoKeywordsString"] as string;
                if (localStr == null)
                {
                    localStr = _matchVideoKeywordsString;
                }

                return localStr;
            }
            set
            {
                localSettings.Values["MatchVideoKeywordsString"] = value;
            }
        }

        /// <summary>
        /// 图片保存地址
        /// </summary>
        public static int isStartAfterMatchName
        {
            get
            {
                return Convert.ToInt32(localSettings.Values["isStartAfterMatchName"]);
            }
            set
            {
                localSettings.Values["isStartAfterMatchName"] = value;
            }
        }

        /// <summary>
        /// 应用的启动页面
        /// </summary>
        public static int StartPageIndex
        {
            get
            {
                return Convert.ToInt32(localSettings.Values["StartPageIndex"]);
            }
            set
            {
                localSettings.Values["StartPageIndex"] = value;
            }
        }

        /// <summary>
        /// 是否使用JavDB
        /// </summary>
        public static bool isUseJavDB
        {
            get
            {
                bool useJavDB = true;

                if(localSettings.Values["isUseJavDB"] is bool value)
                {
                    useJavDB = value;
                }

                return useJavDB;
            }
            set
            {
                localSettings.Values["isUseJavDB"] = value;
            }
        }

        /// <summary>
        /// 是否使用JavBus
        /// </summary>
        public static bool isUseJavBus
        {
            get
            {
                bool useJavBus = true;

                if (localSettings.Values["isUseJavBus"] is bool value)
                {
                    useJavBus = value;
                }
                return useJavBus;
            }
            set
            {
                localSettings.Values["isUseJavBus"] = value;
            }
        }

        public static string VlcExePath
        {
            get
            {
                return localSettings.Values["VlcExePath"] as string;
            }
            set
            {
                localSettings.Values["VlcExePath"] = value;
            }
        }

        public static string MpvExePath
        {
            get
            {
                return localSettings.Values["MpvExePath"] as string;
            }
            set
            {
                localSettings.Values["MpvExePath"] = value;
            }
        }

        /// <summary>
        /// 播放方式
        /// </summary>
        public static int PlayerSelection
        {
            get
            {
                int playerSelection = 0;

                if (localSettings.Values["PlayerSelection"] is int value)
                {
                    playerSelection = value;
                }
                return playerSelection;
            }
            set
            {
                localSettings.Values["PlayerSelection"] = value;
            }
        }

        public enum Origin { Local = 0, Web = 1 }

        /// <summary>
        /// 缩略图的显示来源
        /// </summary>
        //private static Origin _thumbnialOrigin = Origin.Local;
        public static int ThumbnailOrigin
        {
            get
            {
                var thumbnialOrigin = localSettings.Values["thumbnialOrigin"];
                if(thumbnialOrigin == null)
                {
                    return (int)Origin.Local;
                }
                else
                {
                    return (int)thumbnialOrigin;
                }
            }
            set
            {
                localSettings.Values["thumbnialOrigin"] = value;
            }
        }

    }
}
