using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXUtilEliminaContaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmCntXUtilEliminaContaDb(IConfiguration config)
           : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXUtilEliminaContaDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
        }

        /// <summary>
        /// Obtiene contabilidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxContabilidadListaDto>> CntxUtil_Contabilidades_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<CntxContabilidadListaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<CntxContabilidadListaDto>(
                    @"SELECT COD_CONTABILIDAD as cod_contabilidad,
                             NOMBRE as nombre
                      FROM CntX_Contabilidades
                      ORDER BY COD_CONTABILIDAD"
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Elimina contabilidades
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CntxUtil_Contabilidades_Eliminar(CntxUtilEliminaContabilidadesRequestDto request)
        {
            var response = new ErrorDto<bool>();
            if (request == null)
            {
                response.Code = -1;
                response.Description = "El parámetro 'request' no puede ser nulo.";
                return response;
            }

            if (!request.cod_empresa.HasValue)
            {
                response.Code = -1;
                response.Description = "El campo 'cod_empresa' es obligatorio.";
                return response;
            }

            if (request.contabilidades == null || !request.contabilidades.Any())
            {
                response.Code = -1;
                response.Description = "Debe enviar al menos una contabilidad a eliminar.";
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                response.Code = -1;
                response.Description = "El campo 'usuario' es obligatorio.";
                return response;
            }

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(request.cod_empresa.Value));

                cn.Open();

                foreach (var codContabilidad in request.contabilidades)
                {
                    cn.Execute(
                        "exec spCntX_Util_Contabilidad_Elimina @codContabilidad, @usuario, @token",
                        new
                        {
                            codContabilidad,
                            usuario = request.usuario,
                            token = "*xHM1tOk3n$"
                        });

                    _mSecurityMainDb.Bitacora(
                        new Galileo.Models.Security.BitacoraInsertarDto
                        {
                            EmpresaId = request.cod_empresa.Value,
                            Usuario = request.usuario,
                            DetalleMovimiento = $"Elimina: Contabilidad [{codContabilidad}]",
                            Movimiento = "Elimina - WEB",
                            Modulo = 20
                        });
                }

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}