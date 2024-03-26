namespace Display.Models.Spider;

public class SpiderNameAndStatus
{
    public enum SpiderSourceName
    {
        Javbus, Jav321, Avmoo, Avsox, Libredmm, Fc2Club, Javdb, Local
    }

    public enum SpiderStates { Ready, Doing, Awaiting, Done }

}