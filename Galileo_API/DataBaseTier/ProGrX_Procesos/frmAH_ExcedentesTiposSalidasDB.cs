using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesTiposSalidasDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesTiposSalidasDB(IConfiguration config)
        {
            _config = config;
        }


        public List<TipoSalidaDto> ExcedenteTipoSalida_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            List<TipoSalidaDto> info = new List<TipoSalidaDto>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT 
                                        cod_salida,
                                        descripcion,
                                        activa,
                                        opcion_sistema as Sistema,
                                        destino_operadora as Id_Operadora,
                                        Destino_Plan as Codigo_Plan,
                                        Destino_Banco,
                                        TIPO_APLICACION_DESC,
                                        PERMITE_RECLASIFICAR,
                                        REQUIERE_PORCENTAJE,
                                        CASE 
                                            WHEN ISNULL(Tipo_Aplicacion, 'F') = 'F' THEN 'Fondo' 
                                            WHEN ISNULL(Tipo_Aplicacion, 'N') = 'N' THEN 'Ninguna'
                                            WHEN ISNULL(Tipo_Aplicacion, 'T') = 'T' THEN 'Transferencia'
                                            ELSE 'Desconocido'
                                        END AS TIPO,
                                        PLAN_DESC,
                                        BANCO_DESC
                                    FROM 
                                        vExc_Salidas_Tipos
                                    ORDER BY 
                                        Activa DESC,
                                        cod_salida; ";

                    info = connection.Query<TipoSalidaDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

    }
}