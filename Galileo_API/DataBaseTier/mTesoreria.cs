using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public partial class MTesoreria
    {
        private readonly IConfiguration _config;
        private readonly string dirRDLC;

        public MTesoreria(IConfiguration config)
        {
            _config = config;
            dirRDLC = _config.GetSection("AppSettings").GetSection("RutaRDLC").Value ?? string.Empty;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocumentos_Obtener(int CodEmpresa)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesTiposDocumentosObtener).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCbo(int CodEmpresa, string usuario, int banco, int contabilidad)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesUnidadesCargaUsuario, new
                {
                    contabilidad,
                    banco,
                    usuario
                }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCbo(int CodEmpresa, string usuario, int banco)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesConceptosCargaUsuario, new { banco, usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<bool> fxTesTipoAccesoValida(int CodEmpresa, string vBanco, string vUsuario, string vTipo, string vGestion = "S")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<bool> { Code = 0, Result = false };

            try
            {
                using var connection = new SqlConnection(conn);

                string permiso = Mappers.GestionFromCodigo(vGestion);
                string query = Sql.GetTesTipoAccesoValidaByPermiso(permiso);

                int count = connection.QueryFirstOrDefault<int>(query, new { banco = vBanco, usuario = vUsuario, tipo = vTipo });
                resp.Result = count > 0;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = false;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGestion(int CodEmpresa, string usuario, string gestion)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);

                string permiso = Mappers.NormalizePermiso(gestion);
                string query = Sql.GetTesBancoCargaCboAccesoGestionByPermiso(permiso);

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<string> fxTesBancoDocsValor(int CodEmpresa, int vBanco, string vTipo, string vCampo = "Comprobante")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string> { Code = 0, Result = "" };

            try
            {
                using var connection = new SqlConnection(conn);

                string campo = Mappers.NormalizeBancoDocsCampo(vCampo);

                resp.Result = connection.QueryFirstOrDefault<string>(
                    Sql.TesBancoDocsCampoPorTipoBanco,
                    new { campo, banco = vBanco, tipo = vTipo }
                ) ?? "";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAcceso(int CodEmpresa, string Usuario, int Banco, string? Tipo = "S")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);

                string permiso = Mappers.GestionFromCodigo(Tipo ?? "S");
                string query = Sql.GetTesTiposDocsCargaCboAccesoByPermiso(permiso);

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco = Banco, usuario = Usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCboAccesoFirmas(int CodEmpresa, string Usuario, int Banco, string? Tipo = "S")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);

                string permiso = Mappers.GestionFromCodigo(Tipo ?? "S");
                string query = Sql.GetTesTiposDocsCargaCboAccesoFirmasByPermiso(permiso);

                resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { banco = Banco, usuario = Usuario }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<long> fxTesTipoDocConsec(int CodEmpresa, int id_banco, string tipo, string avance = "+", string plan = "-sp-")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<long> { Code = 0, Result = 0 };

            try
            {
                using var connection = new SqlConnection(conn);

                plan = Mappers.NormalizePlan(tipo, plan);

                string selectSql = plan == "-sp-" ? Sql.TesBancoDocsConsecutivo : Sql.TesBancoPlanesTeConsecutivo;
                long current = connection.QueryFirstOrDefault<long>(selectSql, new { Tipo = tipo, Banco = id_banco, Plan = plan });

                long consecutivo = avance switch
                {
                    "+" => current + 1,
                    "-" => current - 1,
                    "/" => current,
                    _ => current
                };

                resp.Result = consecutivo;

                if (avance != "/")
                {
                    string updateSql = plan == "-sp-" ? Sql.UpdateTesBancoDocsConsecutivoByAvance : Sql.UpdateTesBancoPlanesTeNumeroTeByAvance;
                    connection.Execute(updateSql, new { avance, Tipo = tipo, Banco = id_banco, Plan = plan });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = 0;
            }

            return resp;
        }

        public ErrorDto<long> fxTesTipoDocConsecInterno(int CodEmpresa, int id_banco, string tipo, string avance = "+", string plan = "-sp-")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<long> { Code = 0, Result = 0 };

            try
            {
                using var connection = new SqlConnection(conn);

                plan = Mappers.NormalizePlan(tipo, plan);

                string selectSql = plan == "-sp-" ? Sql.TesBancoDocsConsecutivoDet : Sql.TesBancoPlanesTeNumeroInterno;
                long current = connection.QueryFirstOrDefault<long>(selectSql, new { Tipo = tipo, Banco = id_banco, Plan = plan });

                long consecutivo = avance switch
                {
                    "+" => current + 1,
                    "-" => current - 1,
                    "/" => current,
                    _ => current
                };

                resp.Result = consecutivo;

                if (avance != "/")
                {
                    string updateSql = plan == "-sp-" ? Sql.UpdateTesBancoDocsConsecutivoDetByAvance : Sql.UpdateTesBancoPlanesTeNumeroInternoByAvance;
                    connection.Execute(updateSql, new { avance, Tipo = tipo, Banco = id_banco, Plan = plan });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = 0;
            }

            return resp;
        }

        public ErrorDto<string> fxTesTipoDocExtraeDato(int CodEmpresa, int Banco, string TipoDoc, string Campo)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<string> { Code = 0, Description = "", Result = "" };

            try
            {
                using var connection = new SqlConnection(conn);

                string campo = Mappers.NormalizeBancoDocsCampo(Campo);

                resp.Result = connection.QueryFirstOrDefault<string>(
                    Sql.TesBancoDocsCampoPorTipoBanco,
                    new { campo, banco = Banco, tipo = TipoDoc }
                ) ?? "";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }

            return resp;
        }

        public ErrorDto<TesArchivosEspecialesData> sbCargaArchivosEspeciales(int CodEmpresa, int banco)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesArchivosEspecialesData> { Code = 0, Result = new TesArchivosEspecialesData() };

            try
            {
                using var connection = new SqlConnection(conn);

                var archivosData = connection.QueryFirstOrDefault<TesBancosArchivosData>(Sql.TesBancosArchivosEspeciales, new { banco });

                if (archivosData != null)
                {
                    string archivoFirmas = Path.Combine(dirRDLC, CodEmpresa.ToString(), archivosData.archivo_cheques_firmas) ?? "";
                    string archivoSinFirmas = Path.Combine(dirRDLC, CodEmpresa.ToString(), archivosData.archivo_cheques_sin_firmas) ?? "";

                    if (archivosData.utiliza_formato_especial == 1)
                    {
                        response.Result.chequesFirmas = File.Exists(archivoFirmas) ? archivosData.archivo_cheques_firmas : "Banking_DocFormat01";
                        response.Result.chequesSinFirmas = File.Exists(archivoSinFirmas) ? archivosData.archivo_cheques_sin_firmas : "Banking_DocFormat02";
                    }
                    else
                    {
                        response.Result.chequesFirmas = "Banking_DocFormat01";
                        response.Result.chequesSinFirmas = "Banking_DocFormat02";
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

        public ErrorDto sbTesBancosAfectacion(int CodEmpresa, int vSolicitud, string vTipo = "E")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var connection = new SqlConnection(conn);
                connection.Execute(Sql.TesAfectaBancos, new { solicitud = vSolicitud, tipo = vTipo });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesBancosActivos).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto sbTesBitacoraEspecial(int CodEmpresa, int pSolicitud, string pMovimiento, string pDetalle, string Usuario)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                using var connection = new SqlConnection(conn);
                connection.Execute(Sql.TesBitacora, new
                {
                    solicitud = pSolicitud,
                    movimiento = pMovimiento,
                    detalle = pDetalle.Length > 150 ? pDetalle.Substring(0, 150) : pDetalle,
                    usuario = Usuario
                });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                switch ((tipo ?? "").Trim().ToLowerInvariant())
                {
                    case "estado":
                        resp.Result = new()
                        {
                            new() { item = "T", descripcion = "Todos" },
                            new() { item = "S", descripcion = "Solicitados" },
                            new() { item = "E", descripcion = "Emitidos" },
                            new() { item = "A", descripcion = "Anulados" }
                        };
                        break;

                    case "busqueda":
                        resp.Result = new()
                        {
                            new() { item = "T", descripcion = "Todos" },
                            new() { item = "1", descripcion = "Por Número de Caso / Solicitud" },
                            new() { item = "2", descripcion = "Por Nombre Beneficiario" },
                            new() { item = "3", descripcion = "Por Número de Documento" },
                            new() { item = "4", descripcion = "Por Número de Referencia (OP)" }
                        };
                        break;

                    case "documento":
                        resp.Result = new()
                        {
                            new() { item = "C", descripcion = "Cheques" },
                            new() { item = "T", descripcion = "Transferencias" },
                            new() { item = "R", descripcion = "Reporte" }
                        };
                        break;

                    default:
                        resp.Result = new() { new() { item = "T", descripcion = "Todos" } };
                        resp.Description = "No se encontró el pTipo de Combo que se desea llenar.";
                        break;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesUnidadesCargaGeneral, new { contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int Banco)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesTiposDocsPorBanco, new { banco = Banco }).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>> { Code = 0, Result = new() };

            try
            {
                using var connection = new SqlConnection(conn);
                resp.Result = connection.Query<DropDownListaGenericaModel>(Sql.TesConceptosGeneral).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        public class ActualizaCCParams
        {
            public string? Codigo { get; set; }
            public string? Tipo { get; set; }
            public string? Documento { get; set; }
            public int Banco { get; set; }
            public object? OP { get; set; }
            public string? Modulo { get; set; }
            public string? SubModulo { get; set; }
            public int Referencia { get; set; }
        }

        public ErrorDto sbTESActualizaCC(int CodEmpresa, ActualizaCCParams parametros)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = "" };

            try
            {
                if (parametros.Modulo?.Trim() != "CC" || parametros.SubModulo?.Trim() != "C")
                {
                    resp.Code = -1;
                    resp.Description = "Módulo o Submódulo inválido";
                    return resp;
                }

                using var connection = new SqlConnection(conn);

                if (parametros.Referencia > 0)
                {
                    connection.Execute(Sql.UpdateDesembolsosBancoDoc, new
                    {
                        Banco = parametros.Banco,
                        Tipo = parametros.Tipo,
                        Documento = parametros.Documento,
                        Codigo = parametros.Codigo
                    });
                }
                else
                {
                    string documentoReferido = $"{parametros.Tipo}-{parametros.Documento}";
                    connection.Execute(Sql.UpdateRegCreditosBancoDoc, new
                    {
                        Banco = parametros.Banco,
                        documentoReferido,
                        OP = parametros.OP
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public string fxTesParametro(int CodEmpresa, string xCodigo)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(conn);
                return connection.QueryFirst<string>(Sql.TesParametroPorCodigo, new { codigo = xCodigo });
            }
            catch
            {
                return "";
            }
        }

        public ErrorDto<bool> fxValidaEmpresaSinpe(int CodEmpresa)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = false };

            try
            {
                using var connection = new SqlConnection(conn);
                result.Result = connection.QueryFirst<bool>(Sql.EmpresaSinpeActivo, new { empresa = CodEmpresa });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesBancoValida(int CodEmpresa, int vBanco, string vUsuario)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(conn);
                int resp = connection.QueryFirstOrDefault<int>(Sql.TesBancoValida, new { usuario = vUsuario, banco = vBanco });

                if (resp <= 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesConceptoValida(int CodEmpresa, int vBanco, string vUsuario, string vConcepto)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(conn);
                int resp = connection.QueryFirstOrDefault<int>(Sql.TesConceptoValida, new { banco = vBanco, usuario = vUsuario, concepto = vConcepto });

                if (resp <= 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesUnidadValida(int CodEmpresa, int vBanco, string vUsuario, string vUnidad)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(conn);
                int resp = connection.QueryFirstOrDefault<int>(Sql.TesUnidadValida, new { banco = vBanco, usuario = vUsuario, unidad = vUnidad });

                if (resp <= 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public ErrorDto<bool> fxTesDocumentoVerifica(int CodEmpresa, int vBanco, string vtipo, string vDocumento)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(conn);
                int resp = connection.QueryFirstOrDefault<int>(Sql.TesDocumentoExisteNoPendiente, new { banco = vBanco, tipo = vtipo, documento = vDocumento });

                if (resp > 0)
                {
                    result.Code = -1;
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public static string fxTESCifrado(string vClave)
        {
            if (string.IsNullOrEmpty(vClave))
                return string.Empty;

            var sb = new StringBuilder(vClave.Length);
            for (int i = 0; i < vClave.Length; i++)
                sb.Append((char)(vClave[i] + 7));
            return sb.ToString();
        }

        public static string fxStringCifrado(string pCadena)
        {
            if (string.IsNullOrEmpty(pCadena))
                return string.Empty;

            var vResBuilder = new StringBuilder(pCadena.Length * 3);
            for (int i = pCadena.Length - 1; i >= 0; i--)
            {
                int xChar = (int)pCadena[i];
                vResBuilder.Append(xChar.ToString("D3"));
            }

            string vRes = vResBuilder.ToString();
            var deltas = new[] { +1, -5, +7, -13, -2, +3 };
            int vSec = 0;

            var vResXBuilder = new StringBuilder(vRes.Length + vRes.Length / 3);

            for (int i = 0; i < vRes.Length; i += 3)
            {
                int len = Math.Min(3, vRes.Length - i);
                if (!int.TryParse(vRes.AsSpan(i, len), out int num))
                    continue;

                int transformed = num + deltas[vSec];
                vResXBuilder.Append(transformed);

                vSec++;
                if (vSec == deltas.Length) vSec = 0;
            }

            return FxDepuraCadena(vResXBuilder.ToString());
        }

        public static string FxDepuraCadena(string xCadena)
        {
            var vResBuilder = new StringBuilder();

            for (int i = 0; i < xCadena.Length; i += 2)
            {
                string chunk = xCadena.Substring(i, Math.Min(2, xCadena.Length - i));
                if (int.TryParse(chunk, out int num) && num > 31 && num != 39 && num != 34)
                {
                    vResBuilder.Insert(0, (char)num);
                }
            }

            return vResBuilder.ToString();
        }

        public bool fxValidaPermisoUserBancosTipo(int CodEmpresa, int vBanco, string vtipo, string vUsuario, string vPermiso)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(conn);

                string permiso = Mappers.NormalizePermiso(vPermiso);
                string query = Sql.GetValidaPermisoUserBancosTipoByPermiso(permiso);

                return connection.QueryFirstOrDefault<int>(query, new { tipo = vtipo, banco = vBanco, usuario = vUsuario }) > 0;
            }
            catch
            {
                return false;
            }
        }

        public string fxTesTiposDocAsiento(int CodEmpresa, string vTipo)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(conn);
                return connection.QueryFirstOrDefault<string>(Sql.TesTiposDocMovimiento, new { tipo = vTipo }) ?? "";
            }
            catch
            {
                return "";
            }
        }

        public ErrorDto<bool> fxTesCuentaObligatoriaVerifica(int CodEmpresa, int vBanco)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<bool> { Code = 0, Description = "Ok", Result = true };

            try
            {
                using var connection = new SqlConnection(conn);
                int resp = connection.QueryFirstOrDefault<int>(Sql.TesCuentaDestinoObligatoria, new { banco = vBanco });

                if (resp > 0)
                {
                    result.Code = -1;
                    result.Result = true;
                }
                else
                {
                    result.Result = false;
                }
            }
            catch
            {
                result.Code = -1;
                result.Result = false;
            }

            return result;
        }

        public static string fxTesMesDescripcion(int vMes) => vMes switch
        {
            1 => "ENERO",
            2 => "FEBRERO",
            3 => "MARZO",
            4 => "ABRIL",
            5 => "MAYO",
            6 => "JUNIO",
            7 => "JULIO",
            8 => "AGOSTO",
            9 => "SETIEMBRE",
            10 => "OCTUBRE",
            11 => "NOVIEMBRE",
            12 => "DICIEMBRE",
            _ => ""
        };

        public ErrorDto<List<TokenConsultaModel>> spTes_Token_Consulta(int CodEmpresa, string usuario)
        {
            var response = new ErrorDto<List<TokenConsultaModel>> { Code = 0, Result = new() };

            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                response.Result = connection.Query<TokenConsultaModel>(Sql.TesTokenConsulta, new { usuario }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDto spTes_Token_New(int CodEmpresa, string usuario)
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(conn);

                // stored procedure SIN interpolación
                connection.Execute(
                    "spTes_Token_New",
                    new { usuario = usuario.ToUpperInvariant() },
                    commandType: System.Data.CommandType.StoredProcedure);

                response.Description = connection.QueryFirstOrDefault<string>(Sql.TesTokenUltimoPorUsuario, new { usuario = usuario.ToUpperInvariant() }) ?? "";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public void sbCrdOperacionTags(int CodEmpresa, long pOperacion, string pLinea, string pTag, string pUsuario, string? pAsignado = "", string? pNotas = "")
        {
            string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(conn);

                connection.Execute(
                    "[spCrdOperacionTagRegistra]",
                    new
                    {
                        Operacion = pOperacion,
                        CrdLinea = pLinea,
                        Tag = pTag,
                        Usuario = pUsuario,
                        Asignado = pAsignado,
                        Notas = pNotas
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }
    }
}