using Dapper;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public partial class MTesoreria
    {
        private readonly IConfiguration _config;
        private readonly string dirRDLC;
        private readonly MSecurityMainDb DBBitacora;

        public MTesoreria(IConfiguration config)
        {
            _config = config;
            dirRDLC = _config.GetSection("AppSettings").GetSection("RutaRDLC").Value ?? string.Empty;
            DBBitacora = new MSecurityMainDb(config);
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
            var resp = new ErrorDto<string> { Code = 0, Description = TipoDoc, Result = "" };

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

                string baseDir = Path.GetFullPath(Path.Combine(dirRDLC, CodEmpresa.ToString()));

                string nombreFirmas = Path.GetFileName(archivosData!.archivo_cheques_firmas ?? string.Empty);
                string nombreSinFirmas = Path.GetFileName(archivosData.archivo_cheques_sin_firmas ?? string.Empty);

                string archivoFirmas = Path.GetFullPath(Path.Combine(baseDir, nombreFirmas));
                string archivoSinFirmas = Path.GetFullPath(Path.Combine(baseDir, nombreSinFirmas));

                string baseDirWithSep = baseDir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

                bool existeFirmas =
                    archivoFirmas.StartsWith(baseDirWithSep, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(archivoFirmas);

                bool existeSinFirmas =
                    archivoSinFirmas.StartsWith(baseDirWithSep, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(archivoSinFirmas);

                if (!existeSinFirmas || !existeFirmas)
                {
                    response.Code = -1;
                    response.Description += $"Archivo no encontrado en ruta segura: {archivoSinFirmas}. ";
                }

                if (archivosData.utiliza_formato_especial == 1)
                {
                    response.Result.chequesFirmas = File.Exists(archivoFirmas) ? archivosData.archivo_cheques_firmas! : "Banking_DocFormat01";
                    response.Result.chequesSinFirmas = File.Exists(archivoSinFirmas) ? archivosData.archivo_cheques_sin_firmas! : "Banking_DocFormat02";
                }
                else
                {
                    response.Result.chequesFirmas = "Banking_DocFormat01";
                    response.Result.chequesSinFirmas = "Banking_DocFormat02";
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

        public ErrorDto sbTesEmitirDocumento(
           int CodEmpresa, string vUsuario, int vModulo, int vSolicitud, string vDocumento = "", DateTime? vFecha = null)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = ""
            };
            try
            {
                string query = "";
                using var connection = new SqlConnection(stringConn);
                query = @"SELECT C.monto, C.nsolicitud, T.doc_auto, T.comprobante,
                          ISNULL(B.firmas_desde,0) AS Firmas_Desde, B.Lugar_Emision,
                          X.descripcion AS TipoX, ISNULL(B.firmas_hasta,0) AS Firmas_Hasta,
                          dbo.MyGetdate() AS FechaX, C.id_Banco, C.tipo, C.modulo, 
                          C.op, C.referencia, C.codigo, C.subModulo, C.cod_divisa
                           FROM Tes_Transacciones C
                           INNER JOIN Tes_Bancos B ON C.id_Banco = B.id_banco
                           INNER JOIN tes_banco_docs T ON B.id_Banco = T.id_Banco AND C.tipo = T.tipo
                           INNER JOIN tes_tipos_doc X ON T.tipo = X.tipo
                           WHERE C.nsolicitud = @solicitud";

                var data = connection.QueryFirstOrDefault<MTesTransaccionDto>(query, new { solicitud = vSolicitud });

                if (data == null)
                {
                    response.Code = -3;
                    response.Description = "No se encontró la solicitud especificada.";
                }
                else
                {
                    DateTime fechaEmision = vFecha ?? data.fechaX;
                  
                    return ProcesarComprobante(
                           connection,
                           new ProcesarComprobanteParametros
                           {
                                 codEmpresa = CodEmpresa,
                                 usuario = vUsuario,
                                 modulo = vModulo,
                                 solicitud = vSolicitud,
                                 documentoManual = vDocumento
                           },
                           data,
                           fechaEmision);
                }


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        private ErrorDto ProcesarComprobante(
                SqlConnection connection,
                ProcesarComprobanteParametros parametros,
                MTesTransaccionDto data,
                DateTime fechaEmision)
                    {
                        return data.comprobante switch
                        {
                            "01" or "02" or "03" => ProcesarDocumento010203(
                                connection, parametros, data, fechaEmision),

                            "04" => Error(-1, "Las Transferencias Electrónicas no se pueden procesar directamente..."),

                            _ => Error(-1, $"Comprobante no soportado: {data.comprobante}")
                        };
        }

        private ErrorDto ProcesarDocumento010203(
            SqlConnection connection,
            ProcesarComprobanteParametros parametros,
            MTesTransaccionDto data,
            DateTime fechaEmision)
        {
            var tipo = data.tipo;

            string consecutivo = data.doc_auto
                ? fxTesTipoDocConsec(parametros.codEmpresa, data.id_banco, tipo, "+").Result.ToString()
                : string.Empty;

            string documentoFinal = data.doc_auto ? consecutivo : parametros.documentoManual ?? string.Empty;

            ActualizarTransaccionEmitida(
                connection,
                parametros.solicitud,
                parametros.usuario ?? string.Empty,
                fechaEmision,
                documentoFinal ?? string.Empty,
                debeActualizarDocumento: data.doc_auto || !string.IsNullOrWhiteSpace(parametros.documentoManual));

            PostActualizarTransaccion(parametros.codEmpresa, parametros.usuario ?? string.Empty, parametros.modulo, parametros.solicitud, data, tipo, consecutivo);

            return Ok();
        }

        private void ActualizarTransaccionEmitida(
            SqlConnection connection,
            int solicitud,
            string usuario,
            DateTime fechaEmision,
            string documento,
            bool debeActualizarDocumento)
        {
            const string sqlConDocumento = @"
                    UPDATE Tes_Transacciones
                    SET Estado = 'I',
                        Fecha_Emision = @fecha,
                        Ubicacion_Actual = 'T',
                        Fecha_Traslado = @fecha,
                        User_Genera = @usuario,
                        NDocumento = @documento
                    WHERE nsolicitud = @solicitud";

                        const string sqlSinDocumento = @"
                    UPDATE Tes_Transacciones
                    SET Estado = 'I',
                        Fecha_Emision = @fecha,
                        Ubicacion_Actual = 'T',
                        Fecha_Traslado = @fecha,
                        User_Genera = @usuario
                    WHERE nsolicitud = @solicitud";

            var sql = debeActualizarDocumento ? sqlConDocumento : sqlSinDocumento;

            connection.Execute(sql, new
            {
                fecha = fechaEmision.ToString("yyyy-MM-dd"),
                usuario,
                documento,
                solicitud
            });
        }

        private void PostActualizarTransaccion(
            int codEmpresa,
            string usuario,
            int modulo,
            int solicitud,
            MTesTransaccionDto data,
            string tipo,
            string consecutivo)
        {
            sbTesBancosAfectacion(codEmpresa, solicitud, "E");

            DBBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Genero Solicitud {solicitud}",
                Movimiento = "Genera - WEB",
                Modulo = modulo
            });

            sbTESActualizaCC(codEmpresa, new ActualizaCCParams
            {
                Codigo = data.codigo,
                Tipo = tipo,
                Documento = consecutivo,
                Banco = data.id_banco,
                OP = data.op != null ? (int)data.op : 0,
                Modulo = data.modulo,
                SubModulo = data.subModulo,
                Referencia = data.referencia != null ? (int)data.referencia : 0
            });
        }

        private static ErrorDto Ok() => new ErrorDto { Code = 0, Description = "" };
        private static ErrorDto Error(int code, string description) =>
    new ErrorDto { Code = code, Description = description };


        public ErrorDto<TesReporteTransferenciaDto> sbTesReporteTransferencia(SqlConnection connection, int CodEmpresa, int vBanco, long vTransac, string? vTipo = "C", string? vDocumento = "TE", string? vPlan = "-sp-")
        {
            
            var resp = new ErrorDto<TesReporteTransferenciaDto>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesReporteTransferenciaDto()
            };
            decimal curMonto = 0;
            long lngCasos = 0;
            string strDivisa = "", vLetra = "";
            try
            {
                string query = "";
                query = $@"select cta as item,descripcion from Tes_Bancos where id_banco = @vBanco";
                var banco = connection.QueryFirstOrDefault(query, new { vBanco });

                if (banco != null)
                {
                    vLetra = "Sirva la Presente para saludarlo y a la vez solicitarle debitar de nuestra cuenta corriente"
                        + " # " + banco.item + " la suma de ¢ ";
                }

                string strSQL = @"select sum(Monto) as Monto,Count(*) as Casos,cod_divisa from Tes_Transacciones 
                            where tipo = @vDocumento and id_banco = @vBanco and documento_Base = @vTransac";
                if (vPlan != "-sp-")
                {
                    strSQL += " and Cod_Plan = @vPlan";
                }
                strSQL += " group by cod_divisa";

                var rs = connection.QueryFirstOrDefault(strSQL,
                    new
                    {
                        vDocumento,
                        vBanco,
                        vTransac,
                        vPlan
                    });
                if (rs != null)
                {
                    curMonto = rs.Monto;
                    lngCasos = rs.Casos;
                    strDivisa = rs.cod_divisa;
                }

                string vMontoLetras = MProGrXAuxiliarDB.NumeroALetras(curMonto).Result + fxDescDivisa(connection, CodEmpresa, strDivisa).Result;

                resp.Result.registros = lngCasos;
                resp.Result.montoLetras = vMontoLetras;
                resp.Result.totalMonto = curMonto;
                resp.Result.fxNombre = fxTesParametro(CodEmpresa, "01");
                resp.Result.fxPuesto = fxTesParametro(CodEmpresa, "02");
                resp.Result.fxDepartamento = fxTesParametro(CodEmpresa, "03");
                resp.Result.letras1 = vLetra;

                //if (vTipo == "C")
                //{
                    
                //}
                //else
                //{
                //    // No additional processing for tipos distintos de "C":
                //    // se mantiene la respuesta con los valores por defecto inicializados.
                //}
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = new TesReporteTransferenciaDto();
            }
            return resp;
        }

        public ErrorDto<string> fxDescDivisa(SqlConnection connection, int CodEmpresa, string vDivisa)
        {
            
            var resp = new ErrorDto<string>()
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };
            string descripcion = "";
            try
            {
                string query = "";
                query = $@"select top 1 descripcion from CNTX_DIVISAS where cod_divisa = @vDivisa";
                descripcion = connection.QueryFirstOrDefault<string>(query, new { vDivisa }) ?? "";

                if (!string.IsNullOrEmpty(descripcion))
                {
                    string strDescripcion = descripcion.Trim().ToLower();

                    // Tomar la primera palabra
                    string fxCodText = strDescripcion.Split(' ')[0].Trim();

                    if (string.IsNullOrEmpty(fxCodText))
                    {
                        fxCodText = strDescripcion;
                    }

                    // Normalizar capitalización
                    fxCodText = char.ToUpper(fxCodText[0]) + fxCodText.Substring(1);

                    // Normalizar capitalización
                    fxCodText = char.ToUpper(fxCodText[0]) + fxCodText.Substring(1);
                    fxCodText = fxCodText.Trim();

                    char ultima = fxCodText[fxCodText.Length - 1];
                    // Regla básica: vocal → "s", consonante → "es" (pluralización)
                    if ("aeiouáéíóú".Contains(char.ToLower(ultima)))
                    {
                        fxCodText += "s";
                    }
                    else
                    {
                        fxCodText += "es";
                    }

                    resp.Result = string.IsNullOrEmpty(fxCodText) ? strDescripcion : fxCodText;
                }
                else
                {
                    resp.Result = " Colones";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = "";
            }
            return resp;
        }

    }
}