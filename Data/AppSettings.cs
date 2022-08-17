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

        private static string _matchVideoKeywordsString = "aarm,abp,abs,abw,adn,ajvr,aoz,ap,urvrsp,apaa,apak,apkh,apns,ara,atfb,atid,avop,avsa,bahp,baze,bf,bgn,bijn,bnst,booty,bta,btha,cafr,cami,cand,capi,cawd,cbx,cemd,cesd,chn,cjod,cmd,crvr,csdx,cspl,cute,cwpbd,czbd,dandy,dasd,ddff,ddhh,ddk,ddkh,ddob,dgcemd,dgcesd,dioguitar,dldss,dmdg,docp,doks,dpmx,dsambd,dsvr,dt,dtw,dv,dvaj,ebod,ebvr,endx,esk,eyan,ezd,fanh,fdd,fffs,fneo,fsdss,fset,fwdv,G,gachi,gachig,gachip,gachippv,gana,gaor,garea,gdhh,gent,gnab,gns,gopj,gvg,gzap,hbad,hey,hhh,hmn,hnd,hnvr,hodv,hoi,hoiz,homa,hunta,huntb,hxad,hxae,hzgd,ibw,idbd,iesp,ing,instv,inu,ion,ipit,ipvr,ipx,ipz,jac,japornxxx,josi,jpsvr,jufe,jukf,jul,juvr,jux,juy,jvr,k,kane,kavr,kawd,kimu,kird,krnd,ktb,ktds,kuse,kvr,lafbd,lcbd,lcdv,llr,lol,love,lpbr,lulu,luxu,lzdm,maan,madm,man,mao,mas,maxvrh,mcb,mcbd,mcdv,mct,mdb,mdbk,mdtm,mdvr,meyd,mfcs,miaa,miad,miae,mide,midv,mifd,migd,milk,mimk,mird,mism,mist,mium,mkmp,mmks,mmnt,mmus,mogi,mopg,msd,msfh,mstd,mtall,mvsd,mxbd,mxgs,myhd,mywife,n,nacr,naka,nhdta,nhdtb,nitr,nmc,nps,nsfs,nsps,ntk,ntr,nttr,oae,ofje,oigs,okad,onez,ongbak,oni,orec,oretd,pb,pbd,pgd,pgm,pkpd,pppd,pppe,ppt,ppvr,pred,prtd,prvr,psz,pt,pxh,raw,rbd,real,rebd,rebdb,rhts,roe,room,royd,rpin,rtp,s2mbd,saba,sama,savr,scp,sdde,sdjs,sdmf,sdmm,sdnm,sdsi,sero,sgk,sgla,shkd,shn,shpv,shs,sim,simm,siro,sis,sivr,skmj,skyhd,smbd,smd,snis,soe,spc,spro,sps,sqte,srs,srtd,ssis,ssni,sspd,star,stars,stsk,supa,supd,svdvd,sw,sweet,sxar,t,tar,tek,tikp,tkwa,tmavr,tmvi,top,tppn,trg,tsf,tus,ud,ueh,ure,urvk,usba,ve,vec,vema,venu,venx,vgd,vip,vrkm,vrtm,wa,waaa,wanz,wat,wbdv,wdi,wps,xrw,xss,xvsr,ymdd,ymds,yrh,yrll,yrz,zmen";
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

        //应用的启动页面
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

        public enum Origin { Local = 0 , Web = 1 }

        /// <summary>
        /// 缩略图的显示来源
        /// </summary>
        private static Origin _thumbnialOrigin = Origin.Local;
        public static int ThumbnailOrigin
        {
            get
            {
                return (int)_thumbnialOrigin;
            }
            set
            {
                _thumbnialOrigin = (Origin)value;
            }
        }

    }
}
