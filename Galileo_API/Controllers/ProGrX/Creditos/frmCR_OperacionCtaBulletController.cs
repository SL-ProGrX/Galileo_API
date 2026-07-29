using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCrOperacionCtaBulletController
        : ControllerBase
    {
        private readonly FrmCrOperacionCtaBulletBl _bl;

        public FrmCrOperacionCtaBulletController(
            IConfiguration config)
        {
            _bl =
                new FrmCrOperacionCtaBulletBl(
                    config);
        }

        [HttpGet(
            "CrOperacionCtaBullet_Operacion_Obtener")]
        public ErrorDto<CrOperacionCtaBulletData>
            CrOperacionCtaBullet_Operacion_Obtener(
                int codEmpresa,
                int operacion)
        {
            return _bl
                .CrOperacionCtaBullet_Operacion_Obtener(
                    codEmpresa,
                    operacion);
        }

        [HttpPost(
            "CrOperacionCtaBullet_Guardar")]
        public ErrorDto
            CrOperacionCtaBullet_Guardar(
                int codEmpresa,
                CrOperacionCtaBulletGuardarRequest request)
        {
            return _bl
                .CrOperacionCtaBullet_Guardar(
                    codEmpresa,
                    request);
        }
    }
}