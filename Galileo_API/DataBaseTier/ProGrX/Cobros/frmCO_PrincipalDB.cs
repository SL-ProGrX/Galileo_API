using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;


namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOPrincipalDB
    {
        private readonly PortalDB _portalDb;

        public FrmCOPrincipalDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<OperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            var response = new ErrorDto<List<OperacionBusquedaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                                SELECT TOP 10
                                    id_solicitud AS operacion,
                                    RTRIM(codigo) AS codigo,
                                    RTRIM(cedula) AS cedula,
                                    montoapr,
                                    saldo
                                FROM reg_creditos
                                WHERE estadosol = 'F'
                                ORDER BY id_solicitud
                            ";

                response.Result = cn.Query<OperacionBusquedaDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        public ErrorDto<OperacionConsultarDto> Operacion_Consultar(int codEmpresa, int operacion)
        {
            var response = new ErrorDto<OperacionConsultarDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"SELECT 
                            rc.id_solicitud AS operacion,
                            CASE 
                                WHEN rc.estadosol = 'F' THEN 'NORMAL'
                                ELSE 'OTRO'
                            END AS descripcion,
                            CASE 
                                WHEN rc.estadosol = 'F' THEN 'NO'
                                ELSE 'SI'
                            END AS estado,
                            c.codigo AS linea,
                            ISNULL(c.DESCRIPCION_LINEA, c.DESCRIPCION) AS lineaDescripcion,

                            rc.cedula AS identificacion,
                            RTRIM(s.nombre) AS identificacionDescripcion

                        FROM reg_creditos rc

                        LEFT JOIN catalogo c 
                            ON rc.codigo = c.codigo

                        LEFT JOIN socios s 
                            ON rc.cedula = s.cedula

                        WHERE rc.id_solicitud = @operacion
                                ";

                response.Result = cn.QueryFirstOrDefault<OperacionConsultarDto>(sql, new { operacion });
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