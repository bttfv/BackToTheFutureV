using System.Collections.Generic;
using GTA.Math;

namespace BackToTheFutureV
{
    public enum TimeTravelPhase
    {
        Completed = 0,
        OpeningWormhole = 1,
        InTime = 2,
        Reentering = 3        
    }

    public enum ReenterType
    {
        Normal,
        Spawn,
        Forced
    }

    public enum TimeTravelType
    {
        Cutscene,
        Instant,
        RC
    }

    public class Constants
    {
        // All these offsets we're extracted from dummies that were made in 3Ds Max

        // Fire trail in shape of "99" used when a time machine gets strike by lighting
        public static readonly Vector3[] FireTrails99Offsets = new Vector3[39]
        {
            new Vector3(-0.755061f, 1.651605f, -0.02032154f),
            new Vector3(-0.755047f, 1.765048f, -0.0212798f),
            new Vector3(-0.7550347f, 1.878489f, -0.02215795f),
            new Vector3(-0.7550243f, 1.991928f, -0.02288545f),
            new Vector3(-0.7550176f, 2.105363f, -0.02338029f),
            new Vector3(-0.7550131f, 2.218791f, -0.02355667f),
            new Vector3(-0.7550124f, 2.33221f, -0.02332497f),
            new Vector3(-0.7550139f, 2.445619f, -0.02259558f),
            new Vector3(-0.7550696f, 2.558903f, -0.02126825f),
            new Vector3(-0.7550794f, 2.672282f, -0.01921738f),
            new Vector3(-0.7550399f, 2.785753f, -0.0163248f),
            new Vector3(-0.7550461f, 2.899086f, -0.01244536f),
            new Vector3(-0.7549998f, 3.012501f, -0.007422969f),
            new Vector3(-0.7550375f, 3.125654f, -0.00105048f),
            new Vector3(-0.755052f, 3.238759f, 0.006885365f),
            new Vector3(-0.7550315f, 3.351803f, 0.01665901f),
            new Vector3(-0.7550484f, 3.464545f, 0.02859843f),
            new Vector3(-0.7550284f, 3.577074f, 0.04311036f),
            new Vector3(-0.7550181f, 3.689133f, 0.06073608f),
            new Vector3(-0.7550426f, 3.800449f, 0.08212345f),
            new Vector3(-0.7550373f, 3.910842f, 0.1081744f),
            new Vector3(-0.7549922f, 4.019742f, 0.1401516f),
            new Vector3(-0.7550288f, 4.124271f, 0.183698f),
            new Vector3(-0.7550554f, 4.21863f, 0.2460442f),
            new Vector3(-0.7550278f, 4.289472f, 0.3337793f),
            new Vector3(-0.755022f, 4.316851f, 0.4428475f),
            new Vector3(-0.7550405f, 4.3048f, 0.5551998f),
            new Vector3(-0.7550387f, 4.256822f, 0.6573231f),
            new Vector3(-0.7550518f, 4.175659f, 0.7356399f),
            new Vector3(-0.7550058f, 4.071905f, 0.7797211f),
            new Vector3(-0.7550583f, 3.95907f, 0.7891212f),
            new Vector3(-0.7550382f, 3.847078f, 0.7736835f),
            new Vector3(-0.7550091f, 3.747692f, 0.7211241f),
            new Vector3(-0.7550296f, 3.684013f, 0.6285456f),
            new Vector3(-0.7550266f, 3.660522f, 0.5180218f),
            new Vector3(-0.7550222f, 3.664883f, 0.4049845f),
            new Vector3(-0.7550736f, 3.708662f, 0.3014302f),
            new Vector3(-0.7550143f, 3.792399f, 0.2258026f),
            new Vector3(-0.7550426f, 3.894026f, 0.1759465f)
        };

        // Sparks that fly around of car, have blue color for bttf1/2 and red for bttf3
        public static readonly List<List<Vector3>> SparkOffsets = new List<List<Vector3>>()
        {
            new List<Vector3>()
            {
                new Vector3(0, 2.512924f, 0.719179f),
                new Vector3(0, 1.741328f, 1.112428f),
                new Vector3(0, 0.948842f, 1.461248f),
                new Vector3(0, 0.128709f, 1.738517f),
                new Vector3(0, -0.714542f, 1.934559f),
                new Vector3(0, -1.572115f, 2.05374f),
                new Vector3(0, -2.436388f, 2.106712f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.115846f, 2.439398f, 0.885905f),
                new Vector3(-0.431891f, 1.817174f, 1.39876f),
                new Vector3(-0.761264f, 1.209961f, 1.921057f),
                new Vector3(-1.120302f, 0.638691f, 2.463616f),
                new Vector3(-1.547203f, 0.166296f, 3.047611f)
            },
            new List<Vector3>()
            {
                new Vector3(0.115846f, 2.439398f, 0.885905f),
                new Vector3(0.43189f, 1.817174f, 1.39876f),
                new Vector3(0.761264f, 1.209961f, 1.921058f),
                new Vector3(1.120301f, 0.638691f, 2.463616f),
                new Vector3(1.547203f, 0.166296f, 3.047612f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.39143f, 2.475288f, 0.742706f),
                new Vector3(-0.750809f, 1.783014f, 1.096595f),
                new Vector3(-0.991762f, 0.966849f, 1.25254f),
                new Vector3(-1.185049f, 0.126814f, 1.33482f),
                new Vector3(-1.354231f, -0.721275f, 1.380775f),
                new Vector3(-1.50879f, -1.57308f, 1.404962f),
                new Vector3(-1.65337f, -2.426935f, 1.41441f)
            },
            new List<Vector3>()
            {
                new Vector3(0.510651f, 2.502009f, 0.77206f),
                new Vector3(0.876344f, 1.814953f, 1.120965f),
                new Vector3(1.110193f, 0.994474f, 1.265065f),
                new Vector3(1.29644f, 0.152204f, 1.340689f),
                new Vector3(1.459076f, -0.697358f, 1.383078f),
                new Vector3(1.607347f, -1.550331f, 1.405459f),
                new Vector3(1.74589f, -2.405195f, 1.414376f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.537056f, 2.617612f, 0.651508f),
                new Vector3(-1.126138f, 2.075544f, 0.981871f),
                new Vector3(-1.673436f, 1.495115f, 1.318119f),
                new Vector3(-2.134565f, 0.84859f, 1.661537f),
                new Vector3(-2.461998f, 0.126195f, 2.005894f),
                new Vector3(-2.626427f, -0.654779f, 2.33845f),
                new Vector3(-2.633279f, -1.462411f, 2.648154f),
                new Vector3(-2.511802f, -2.271113f, 2.931375f),
                new Vector3(-2.293917f, -3.068072f, 3.189906f),
                new Vector3(-2.00494f, -3.848973f, 3.42741f)
            },
            new List<Vector3>()
            {
                new Vector3(0.701011f, 2.617612f, 0.651508f),
                new Vector3(1.290093f, 2.075544f, 0.981871f),
                new Vector3(1.837391f, 1.495115f, 1.318119f),
                new Vector3(2.29852f, 0.84859f, 1.661537f),
                new Vector3(2.625953f, 0.126195f, 2.005894f),
                new Vector3(2.790382f, -0.654779f, 2.338449f),
                new Vector3(2.797234f, -1.46241f, 2.648154f),
                new Vector3(2.675757f, -2.271113f, 2.931374f),
                new Vector3(2.457872f, -3.068072f, 3.189907f),
                new Vector3(2.168895f, -3.848972f, 3.42741f)
            },
            new List<Vector3>()
            {
                new Vector3(-0.847276f, 2.517416f, 0.506803f),
                new Vector3(-1.38483f, 1.846296f, 0.599559f),
                new Vector3(-1.769817f, 1.07715f, 0.686075f),
                new Vector3(-2.012557f, 0.250274f, 0.764431f),
                new Vector3(-2.158454f, -0.600096f, 0.836129f),
                new Vector3(-2.240482f, -1.459543f, 0.902991f)
            },
            new List<Vector3>()
            {
                new Vector3(0.482773f, 2.517416f, 0.506803f),
                new Vector3(1.020328f, 1.846296f, 0.599559f),
                new Vector3(1.405314f, 1.07715f, 0.686075f),
                new Vector3(1.648054f, 0.250275f, 0.764431f),
                new Vector3(1.793951f, -0.600096f, 0.836129f),
                new Vector3(1.875979f, -1.459543f, 0.902991f)
            }
        };
    }
}