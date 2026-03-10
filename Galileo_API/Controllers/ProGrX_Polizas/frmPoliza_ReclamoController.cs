using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    public class FrmPolizaReclamoController : ControllerBase
    {
       private readonly FrmPolizaReclamoBL _BL;
    
       public FrmPolizaReclamoController(IConfiguration config)
       {
         _BL = new FrmPolizaReclamoBL(config);
       }

    }
}
