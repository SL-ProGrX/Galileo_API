using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier
{
    public partial class FrmCajasFndaportacionesDB
    {
        /// <summary>Consulta el modo de autorización y el límite real del usuario para aportes.</summary>
        private ErrorDto<FondosRequiereAutorizacionDto> Cajas_FNDAportaciones_Autorizacion_Consultar(
            int codEmpresa, FondosGestionRegistroAddDto request)
        {
            try
            {
                if (_mFndFunciones.fxFndParametro(codEmpresa, "01.1") != "S")
                    return DbHelper.CreateOkResponse(new FondosRequiereAutorizacionDto { autorizado = true });

                var datos = DbHelper.ExecuteSingleQuery<FndAutorizaDto>(
                    _portalDb, codEmpresa, "EXEC spFnd_Autoriza_Datos @Plan, @TipoMov, @Usuario;", default,
                    new { Plan = request.plan, TipoMov = "A", Usuario = request.usuario });
                if (datos.Code != 0 || datos.Result == null)
                    return DbHelper.CreateErrorResponse<FondosRequiereAutorizacionDto>(datos.Description ?? "No se encontró el límite autorizado del plan.");

                return DbHelper.CreateOkResponse(new FondosRequiereAutorizacionDto
                {
                    modo_autorizacion = true,
                    autorizado = datos.Result.autorizado == 1,
                    montomaximo = datos.Result.monto,
                    requiere = request.aporte > datos.Result.monto
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FondosRequiereAutorizacionDto>(ex.Message);
            }
        }

        /// <summary>Valida permisos, límite y estado consultados en BD, sin confiar en los enviados por Angular.</summary>
        private ErrorDto Cajas_FNDAportaciones_Seguridad_Validar(int codEmpresa, FondosAporteAplicarDto request)
        {
            var limite = Cajas_FNDAportaciones_Autorizacion_Consultar(codEmpresa, new FondosGestionRegistroAddDto
            {
                plan = request.plan, usuario = request.usuario, aporte = request.aporte,
                contrato = request.contrato, montoautorizado = 0
            });
            if (limite.Code != 0 || limite.Result == null)
                return new ErrorDto { Code = -1, Description = limite.Description };
            if (!limite.Result.modo_autorizacion)
                return DbHelper.CreateOkResponse();

            var permiso = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa,
                "EXEC spFndSeguridad_ApAnul @Operadora, @Plan, @Usuario;", 0,
                new { Operadora = request.operadora, Plan = request.plan, Usuario = request.usuario });
            if (permiso.Code != 0)
                return new ErrorDto { Code = permiso.Code, Description = permiso.Description };
            if (!limite.Result.autorizado || permiso.Result == 0)
                return new ErrorDto { Code = -1, Description = "El Usuario no tiene nivel de Autorización para realizar este movimiento!" };
            if (!limite.Result.requiere)
                return DbHelper.CreateOkResponse();

            return Cajas_FNDAportaciones_Gestion_Validar(codEmpresa, request.gestionid);
        }

        /// <summary>Verifica en BD que la gestión requerida esté aprobada.</summary>
        private ErrorDto Cajas_FNDAportaciones_Gestion_Validar(int codEmpresa, int gestionId)
        {
            if (gestionId > 0)
            {
                var gestion = Fondos_Gestion_Estado(codEmpresa, gestionId);
                if (gestion.Code != 0)
                    return new ErrorDto { Code = gestion.Code, Description = gestion.Description };
                if (gestion.Result?.Gestion_Id == gestionId &&
                    gestion.Result.Gestion_Estado?.Trim().StartsWith("A", StringComparison.OrdinalIgnoreCase) == true)
                    return DbHelper.CreateOkResponse();
            }
            return new ErrorDto { Code = -1, Description = "- Este movimiento requiere AUTORIZACION, verifique el estado de la misma y/o solicite una!" };
        }
    }
}
