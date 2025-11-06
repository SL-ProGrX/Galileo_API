using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_MovimientosDB
    {
        private readonly IConfiguration _config;

        public frmAH_MovimientosDB(IConfiguration config)
        {
            _config = config;
        }

        public List<MovimientosPatrimonioDto> consultarMovimientos_Obtener(int CodCliente, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            MovimientosPatrimonioFiltros filtros = JsonConvert.DeserializeObject<MovimientosPatrimonioFiltros>(filtroString);

            List<MovimientosPatrimonioDto> info = new List<MovimientosPatrimonioDto>();
            try
            {
                string where = "";

                // Convertir la cadena ISO a DateTimeOffset
                DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.fecha_inicio);
                string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtros.fecha_corte);
                string fechacorte = fecha_corte.ToString("yyyy-MM-dd");


                where = $"where D.fecha BETWEEN '{fechainicio} 00:00:00' AND '{fechacorte} 23:59:59' ";



                if (filtros.cedula != null)
                {



                    where = $"where D.fecha BETWEEN '{fechainicio} 00:00:00' AND '{fechacorte} 23:59:59' AND D.cedula = '{filtros.cedula}'";


                }

                if (filtros.documento != "")
                {



                    where = $"where D.fecha BETWEEN '{fechainicio} 00:00:00' AND '{fechacorte} 23:59:59' AND D.documento = '{filtros.documento}'";

                }





                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT D.*, Sec.DESCRIPCION AS SectorDesc
                                        FROM vSIF_CtrlDoc_Pat_Detalle D
                                        INNER JOIN Socios S ON D.cedula = S.CEDULA
                                        LEFT JOIN AFI_SECTORES Sec ON S.COD_SECTOR = Sec.COD_SECTOR
                                        {where}; ";

                    info = connection.Query<MovimientosPatrimonioDto>(query).ToList();
                }
            }
            catch (Exception)
            {
                info = new List<MovimientosPatrimonioDto>();
            }

            return info;

        }



        public List<DocumentosTransaccionSifDto> Obtener_TipoTransaccion(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<DocumentosTransaccionSifDto> info = new List<DocumentosTransaccionSifDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT 
                                    RTRIM(Tipo_Documento) AS IdX, 
                                    RTRIM(Descripcion) AS ItmX
                                FROM 
                                    sif_documentos
                                WHERE 
                                    Tipo_Documento IN ('ND', 'NC', 'RE', 'LIQ', 'RLIQ', 'PLA', 'ING', 'CAJA', 'CAJARE')
                                ORDER BY 
                                    Descripcion;";

                    info = connection.Query<DocumentosTransaccionSifDto>(query).ToList();

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