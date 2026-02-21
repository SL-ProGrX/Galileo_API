using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXProcesosAddDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly int vModulo = 20;

        public FrmCntXProcesosAddDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config))
        {
        }

        public FrmCntXProcesosAddDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            DBBitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene los asientos disponibles para procesamiento adicional
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CtnXProcesosAddDto>> CntXProcesosAdd_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select * from CntX_Procesos_Add where cod_Contabilidad = @codConta and activo = 1";

            return DbHelper.ExecuteListQuery<CtnXProcesosAddDto>(_portalDb, codEmpresa, query, new { codConta });
        }

        /// <summary>
        /// Procesa los asientos seleccionados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CntXProcesosAdd_Procesar(int codEmpresa, CntXProcesarRequest req)
        {
            try
            {
                foreach (var item in req.lista)
                {
                    var resp = DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        $@"exec {item.sp_name} @codProceso, @codContabilidad, @anio, @mes, @usuario;",
                        new
                        {
                            codProceso = item.cod_proceso,
                            codContabilidad = req.cod_contabilidad,
                            anio = req.periodo_anio,
                            mes = req.periodo_mes,
                            usuario = req.usuario
                        }
                    );

                    if (resp is { Code: not null } && resp.Code != 0)
                    {
                        return resp;
                    }

                    DBBitacora.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = codEmpresa,
                        Usuario = (req.usuario ?? "").ToUpper(),
                        DetalleMovimiento =
                            $"Aplica Proceso Add.: {item.cod_proceso} (Conta.: {req.cod_contabilidad} Periodo.: {req.periodo_anio}-{req.periodo_mes})",
                        Movimiento = "Aplica - WEB",
                        Modulo = vModulo
                    });
                }

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Procesos Adicionales procesados satisfactoriamente!"
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }
    }
}
