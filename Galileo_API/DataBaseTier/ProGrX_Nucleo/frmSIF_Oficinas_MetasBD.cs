using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_Oficinas_MetasBD
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10;
        private readonly mSecurityMainDb _Security_MainDB;


        public frmSIF_Oficinas_MetasBD(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }
        /// <summary>
        /// Consulta de todas las metas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="anio"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<SifOficinasMetaLista> Sif_OficinasMetasLista_Obtener(int CodEmpresa, string oficina, int anio, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<SifOficinasMetaLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifOficinasMetaLista()
                {
                    total = 0,
                    lista = new List<SifOficinasMetaData>()
                }
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSIFOficinaMetasPeriodo '{oficina}',{anio},{anio + 1},'{usuario}'";
                    result.Result.lista = connection.Query<SifOficinasMetaData>(query).ToList();
                    result.Result.total = result.Result.lista.Count();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }

        /// <summary>
        /// Consulta los periodos por oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Sif_OficinasMetasPeriodos_Obtener(int CodEmpresa, string oficina)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                 
                    var query = $@"select Anio_Inicio as 'item',CONCAT(Anio_Inicio, ' - ', ANIO_CORTE) as descripcion from sif_oficina_metas_periodos where cod_oficina ='{oficina}'  order by anio_Corte desc";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                   
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
        /// <summary>
        /// Actualiza las metas por oficina y año
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="usuario"></param>
        /// <param name="metas"></param>
        /// <returns></returns>
        public ErrorDTO Sif_OficinasMetas_Actualizar(int CodEmpresa, string oficina, string usuario, List<SifOficinasMetaData> metas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var meta in metas)
                    {
                        var query = $@"UPDATE sif_oficina_metas
                                    SET mes_meta = @mes_meta,
                                        acumulado_meta = @acumulado_meta,
                                        Actualizado_Fecha = dbo.MyGetdate(),
                                        Actualizado_Usuario = @usuario
                                    WHERE cod_oficina = @cod_oficina  and Anio = @anio and Mes =@mes";
                        connection.Execute(query, new
                        {
                            mes_meta = meta.mes_meta,
                            acumulado_meta = meta.acumulado_meta,
                            usuario = usuario,
                            cod_oficina = oficina,
                            anio = meta.anio,
                            mes = meta.mes
                        });


                    }
               

                    

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}
