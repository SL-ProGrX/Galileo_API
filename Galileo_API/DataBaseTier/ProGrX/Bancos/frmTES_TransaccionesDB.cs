using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransaccionesDb
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria mTesoreria;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;
        private readonly int vModulo = 9;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly FrmCntXConsultaCuentasDb _ConsultaCuentasDB;
        private readonly VerificadorCoreFactory _factory;

        private readonly string descripcion = "descripcion";
        private readonly string nSolicitud = "NSOLICITUD";

        private readonly Dictionary<string, string> whitelist = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["item"] = "item",
            ["descripcion"] = "descripcion",
            ["correo"] = "correo"
        };
 
        public FrmTesTransaccionesDb(IConfiguration config)
        {
            _config = config;
            mTesoreria = new MTesoreria(config);
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _ConsultaCuentasDB = new FrmCntXConsultaCuentasDb(config);
            _factory = new VerificadorCoreFactory(config);
        }

        #region Helpers privados para reducir duplicidad

        private SqlConnection OpenConnection(int codEmpresa)
        {
            var cs = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(cs);
        }

        private ErrorDto<T> WithConn<T>(int codEmpresa, Func<SqlConnection, T> action)
        {
            try
            {
                using var conn = OpenConnection(codEmpresa);
                var result = action(conn);
                return new ErrorDto<T> { Code = 0, Description = "Ok", Result = result };
            }
            catch (Exception ex)
            {
                return new ErrorDto<T> { Code = -1, Description = ex.Message, Result = default };
            }
        }

        private static ErrorDto<T> Ok<T>(T result, string desc = "Ok") =>
            new ErrorDto<T> { Code = 0, Description = desc, Result = result };

        private static ErrorDto<T> Error<T>(string msg, int code = -1) =>
            new ErrorDto<T> { Code = code, Description = msg, Result = default };

        private static ErrorDto OkSimple(string desc = "Ok") =>
            new ErrorDto { Code = 0, Description = desc };

        private static ErrorDto ErrorSimple(string msg, int code = -1) =>
            new ErrorDto { Code = code, Description = msg };

        #endregion

        #region Cargas dropdown / catálogos

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocumentos_Obtener(int CodEmpresa, string Usuario, int id_banco, string? tipo = "S")
            => mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, Usuario, id_banco, tipo);

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Unidades_Obtener(int CodEmpresa, string usuario, int banco, int contabilidad)
            => mTesoreria.sbTesUnidadesCargaCbo(CodEmpresa, usuario, banco, contabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
            => _AuxiliarDB.TiposIdentificacion_Obtener(CodEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int CodEmpresa, string usuario, int banco)
            => mTesoreria.sbTesConceptosCargaCbo(CodEmpresa, usuario, banco);

        public ErrorDto<List<DropDownListaGenericaModel>> TES_BancosCarga_Obtener(int CodEmpresa, string usuario, string gestion)
            => mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);

        #endregion

        #region Consultas varias

        public ErrorDto<List<TesAfectacionDto>> TES_Afectaciones_Obtener(int CodEmpresa, int tesoreria)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"exec spTes_Consulta_Afectacion_Modulos @Solicitud";
                return conn.Query<TesAfectacionDto>(query, new { Solicitud = tesoreria }).ToList();
            });
        }

        public ErrorDto<List<TesBitacoraDto>> TES_Bitacora_Obtener(int CodEmpresa, int tesoreria)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"
                    select H.ID, H.FECHA, H.USUARIO,ISNULL(M.DESCRIPCION,'No identificado') AS MOVIMIENTO,H.DETALLE
                    from TES_HISTORIAL H left join TES_TIPOS_MOVIMIENTOS M on H.COD_MOVIMIENTO = M.COD_MOVIMIENTO
                    WHERE H.NSOLICITUD = @Solicitud";
                return conn.Query<TesBitacoraDto>(query, new { Solicitud = tesoreria }).ToList();
            });
        }

        public ErrorDto<int> TES_Transaccion_Scroll(int CodEmpresa, int scrollCode, string codigo, int contabilidad)
        {
            try
            {
                using var conn = OpenConnection(CodEmpresa);

                var query = @"select Top 1 nsolicitud from Tes_Transacciones C
                              inner join CntX_Unidades U on C.cod_unidad = U.cod_unidad";

                switch (scrollCode)
                {
                    case 0:
                        if (codigo == "") codigo = "0";
                        query += $@" where C.nsolicitud > @codigo AND U.cod_contabilidad = @contabilidad order by C.nsolicitud asc";
                        break;
                    case 1:
                        if (codigo == "0") codigo = "999999999";
                        query += $@" where C.nsolicitud <@codigo AND U.cod_contabilidad = @contabilidad order by C.nsolicitud desc";
                        break;
                }

                var result = conn.Query<int>(query, new { codigo, contabilidad }).FirstOrDefault();
                if (result == 0)
                    return TES_Transaccion_Scroll(CodEmpresa, scrollCode, result.ToString(), contabilidad);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        public ErrorDto<TesTransaccionDto> TES_Transaccion_Obtener(int CodEmpresa, int tesoreria, int contabilidad)
        {
            return WithConn(CodEmpresa, conn =>
            {
                var query = @"exec spTes_Transaccion_Consulta @Solicitud";
                var trx = conn.Query<TesTransaccionDto>(query, new { Solicitud = tesoreria }).FirstOrDefault();

                if (trx != null)
                {
                    trx.detalle = string.Join(" ",
                        trx.detalle1 ?? "",
                        trx.detalle2 ?? "",
                        trx.detalle3 ?? "",
                        trx.detalle4 ?? "",
                        trx.detalle5 ?? ""
                    ).Replace("null", "").Trim();
                }
                else
                {
                    trx.detalle = string.Empty;
                }

                    return trx;
            }) ;
        }

        #endregion

      
    }
}
