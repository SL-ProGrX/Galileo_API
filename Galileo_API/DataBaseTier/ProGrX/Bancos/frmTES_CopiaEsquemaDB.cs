using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_CopiaEsquemaDB
    {
        private readonly IConfiguration? _config;
        private readonly mSecurityMainDb BitacoraDb;

        public frmTES_CopiaEsquemaDB(IConfiguration config)
        {
            _config = config;
            BitacoraDb = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// 'OBJETIVO:      Despliega en pantalla datos principales del # solicitud suministrado por el
        /// '               usuario.
        /// 'REFERENCIAS:   fxDescribeBanco - (Devuelve la descripcion del Banco al que se giro la
        /// '               solicitud)
        /// 'OBSERVACIONES: Ninguna.
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDTO<tesCopiaEsquemaModels> Tes_CopiaEsquema_Obtener(int CodEmpresa, int solicitud, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<tesCopiaEsquemaModels>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.codigo,C.Beneficiario,C.Monto,C.Fecha_Solicitud,C.Tipo,C.Id_Banco
                                   ,C.cod_unidad,C.cod_concepto,U.descripcion as UnidadDesc,X.descripcion as ConceptoDesc
                                   ,T.descripcion as TDocumento,B.descripcion as BancoDesc,
                                   C.CTA_AHORROS as cuentaIBAN , C.CTA_IBAN_ORIGEN as cuentaOrigen, 
                                   C.CORREO_NOTIFICA as correo , C.COD_DIVISA as divisa
                                   , C.TIPO_CED_ORIGEN as tipoId, C.detalle1, C.detalle2, C.detalle3, C.detalle4, C.detalle5
                                    from Tes_Transacciones C inner join CntX_unidades U on C.cod_unidad = U.cod_unidad and cod_Contabilidad = @contabilidad
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_conceptos X on C.cod_concepto = X.cod_concepto
                                    inner join Tes_Bancos B on C.id_banco = B.id_banco
                                    where C.nsolicitud = @solicitud ";

                    response.Result = connection.Query<tesCopiaEsquemaModels>(
                         query,
                         new
                         {
                             solicitud = solicitud,
                             contabilidad = contabilidad
                         }).FirstOrDefault();

                    if (response.Result != null)
                    {
                        response.Result.detalle = string.Join(" ",
                                                    response.Result.detalle1 ?? "",
                                                    response.Result.detalle2 ?? "",
                                                    response.Result.detalle3 ?? "",
                                                    response.Result.detalle4 ?? "",
                                                    response.Result.detalle5 ?? ""
                                                ).Replace("null", "").Trim();
                    }

                    if (response.Result != null)
                    {
                        response.Result.solicitud = solicitud;
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }
    
        /// <summary>
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Duplica una determinada solicitud ya ingresada a Tesoreria. Tambien duplica
        ///'               el detalle de la misma solicitud para la nueva.
        ///'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
        ///'               sbLimpiaDatos - (Limpia los objetos de entrada de datos)
        ///'               fxValidaSolicitud - (Valida que la Solicitud por duplicar contenga
        ///'               identificador de Banco y codigo)
        ///'               fxFechaServidor - (Devuelve la fecha del servidor)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDTO Tes_CopiarEsquema_Guardar(int CodEmpresa, tesCopiaEsquemaModels solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTES_Transaccion_Copia {solicitud.solicitud} , '{solicitud.notas}', '{solicitud.usuario}' ";
                    var tesoleria = connection.Query<int>(query).FirstOrDefault();

                    if (tesoleria > 0)
                    {
                        BitacoraDb.Bitacora(new Models.BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = solicitud.usuario.ToUpper(),
                            DetalleMovimiento = "Solicitud de Copia de Esquema, Solicitud: " + solicitud.solicitud + " A la Sol : " + tesoleria,
                            Movimiento = "Aplica",
                            Modulo = 9 
                        });

                        response.Description = tesoleria.ToString();
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = "No fue posible realizar la Copia de la Solicitud!";
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Método que obtiene una lista de solicitudes de copia de esquema de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<tesCopiaEsquemaLista> Tes_CopiaEsquemaLista_Obtener(int CodEmpresa, int contabilidad, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<tesCopiaEsquemaLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new tesCopiaEsquemaLista()
                {
                    total = 0,
                    lista = new List<tesCopiaEsquemaModels>()
                }
            };
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(C.nsolicitud)  from Tes_Transacciones C inner join CntX_unidades U on C.cod_unidad = U.cod_unidad and cod_Contabilidad = @contabilidad
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_conceptos X on C.cod_concepto = X.cod_concepto
                                    inner join Tes_Bancos B on C.id_banco = B.id_banco ";
                    result.Result.total = connection.Query<int>(query, new { contabilidad = contabilidad }).FirstOrDefault();

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@"WHERE ( 
                                                 NSOLICITUD like '%{filtros.filtro}%' 
                                              OR TIPO like '%{filtros.filtro}%'
                                              OR BENEFICIARIO like '%{filtros.filtro}%'
                                              OR CODIGO like '%{filtros.filtro}%' 
                                          )";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "NSOLICITUD";
                    }

                    if (filtros.sortOrder == 0)
                    {
                        filtros.sortOrder = 1; //Por defecto orden ascendente
                    }

                    //if (filtros.filtro != null)
                    //{
                    //    filtros.filtro = " WHERE ( upper(Beneficiario) LIKE '%" + filtros.filtro.ToUpper() + "%' " +
                    //        " OR nsolicitud LIKE '%" + filtros.filtro + "%' " +
                    //        " OR codigo LIKE '%" + filtros.filtro + "%' ) ";
                    //}

                    //if (filtros.pagina != null)
                    //{
                    //    paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                    //    paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    //}

                    query = $@"select NSOLICITUD, CODIGO, BENEFICIARIO, MONTO, Fecha_Solicitud, TIPO, Id_Banco, COD_UNIDAD, cod_concepto, UnidadDesc, ConceptoDesc, TDocumento, BancoDesc  FROM (
                                   select C.nsolicitud, C.codigo,C.Beneficiario,C.Monto,C.Fecha_Solicitud,C.Tipo,C.Id_Banco
                                   ,C.cod_unidad,C.cod_concepto,U.descripcion as UnidadDesc,X.descripcion as ConceptoDesc
                                   ,T.descripcion as TDocumento,B.descripcion as BancoDesc
                              from Tes_Transacciones C inner join CntX_unidades U on C.cod_unidad = U.cod_unidad and cod_Contabilidad = @contabilidad
                                    inner join tes_tipos_doc T on C.tipo = T.tipo
                                    inner join tes_conceptos X on C.cod_concepto = X.cod_concepto
                                    inner join Tes_Bancos B on C.id_banco = B.id_banco  
                                    )X {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == -1 ? "ASC" : "DESC")} 
                                        OFFSET {filtros.pagina} ROWS
                                      FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<tesCopiaEsquemaModels>(query, new { contabilidad = contabilidad }).ToList();
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

    }
}
