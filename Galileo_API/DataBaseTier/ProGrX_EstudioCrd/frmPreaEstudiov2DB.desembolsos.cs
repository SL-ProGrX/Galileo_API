using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta los desembolsos del expediente. Fiel a VB6 sbDesembolsos_Load
        /// (frmPreaEstudiov2.frm línea ~17393): select * from CRD_PREA_DETALLE_DESEMBOLSOS
        /// where cod_PreAnalisis = &lt;expediente&gt;. La consulta anterior invocaba por error
        /// el SP de guardado (spCrdPreaGuardaDesembolsos), lo cual fallaba en tiempo de ejecución.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string usuario)
        {
            var result = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                result.Result = ConsultarDesembolsosYBancos(connection, cod_preanalisis, usuario, out var sinBancos);

                // No se marca como error: replica el comportamiento de VB6, donde la
                // ausencia de bancos es una advertencia y no bloquea la carga del expediente.
                if (sinBancos)
                {
                    result.Description = "No existen Bancos [Creados/Asignados], verifique en Tesoreria.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2DesembolsosResponse();
            }

            return result;
        }

        private static FrmPreaEstudiov2DesembolsosResponse ConsultarDesembolsosYBancos(
            IDbConnection connection,
            string cod_preanalisis,
            string usuario,
            out bool sinBancos)
        {
            const string sqlDesembolsos = @"select IdX as id_desembolso, cod_Acredor as cod_acredor,
                Ordinario as ordinario, Descripcion as descripcion, Cuota as cuota, Monto as monto
                from CRD_PREA_DETALLE_DESEMBOLSOS where cod_PreAnalisis = @Expediente";
            var desembolsos = connection.Query<FrmPreaEstudiov2DesembolsoDto>(
                sqlDesembolsos,
                new { Expediente = cod_preanalisis.Trim() }
            ).ToList();

            var bancosParameters = new DynamicParameters();
            bancosParameters.Add("@Usuario", usuario?.Trim() ?? string.Empty, DbType.String);

            var bancosRows = connection.Query(
                "spCrd_SGT_Bancos_Desembolso",
                bancosParameters,
                commandType: CommandType.StoredProcedure
            );
            var bancos = new List<FrmPreaEstudiov2DropdownDto>();
            foreach (var row in bancosRows)
            {
                var dict = new Dictionary<string, object>((IDictionary<string, object>)row, StringComparer.OrdinalIgnoreCase);
                var item = GetString(dict, "Id_Banco").Trim();
                if (item.Length == 0)
                {
                    item = GetString(dict, "item").Trim();
                }
                if (item.Length == 0)
                {
                    item = GetString(dict, "IdX").Trim();
                }
                if (item.Length == 0 || bancos.Any(b => b.item == item))
                {
                    continue;
                }

                var descripcion = GetString(dict, "Descripcion").Trim();
                if (descripcion.Length == 0)
                {
                    descripcion = GetString(dict, "descripcion").Trim();
                }
                if (descripcion.Length == 0)
                {
                    descripcion = GetString(dict, "ItmX").Trim();
                }

                bancos.Add(new FrmPreaEstudiov2DropdownDto
                {
                    item = item,
                    descripcion = descripcion.Length == 0 ? "SIN DESCRIPCION" : descripcion
                });
            }

            sinBancos = bancos.Count == 0;

            return new FrmPreaEstudiov2DesembolsosResponse
            {
                desembolsos = desembolsos,
                bancos = bancos
            };
        }

        /// <summary>
        /// Consulta la lista equivalente a VB6 lswD_Lista. Si Ordinario es "Si",
        /// lee crd_prea_Acredores; si es "No", lee CONCEPTO_DESEMB retenibles.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2DesembolsoAcreedorDto>> Prea_frmPreaEstudiov2_Desembolsos_Acreedores_Consultar(
            int codEmpresa,
            bool ordinario,
            string? filtro)
        {
            var response = new ErrorDto<List<FrmPreaEstudiov2DesembolsoAcreedorDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var texto = (filtro ?? string.Empty).Trim();
                var like = $"%{texto}%";

                var sql = ordinario
                    ? @"select
                            rtrim(cod_acredor) as id,
                            rtrim(Nombre) as nombre,
                            rtrim(NOMBRE_GIRO) as nombre_giro,
                            isnull(MODIFICA_NOMBRE_GIRO, 0) as modifica
                        from crd_prea_Acredores
                        where activo = 1
                          and (@Texto = '' or Nombre like @Like)
                        order by Nombre"
                    : @"select
                            rtrim(COD_CONDEB) as id,
                            rtrim(DESCRIPCION) as nombre,
                            rtrim(DESCRIPCION) as nombre_giro,
                            isnull(Modifica, 0) as modifica
                        from CONCEPTO_DESEMB
                        where activo = 1
                          and Retiene = 1
                          and (@Texto = '' or DESCRIPCION like @Like)
                        order by DESCRIPCION";

                response.Result = connection.Query<FrmPreaEstudiov2DesembolsoAcreedorDto>(
                    sql,
                    new { Texto = texto, Like = like }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = [];
            }

            return response;
        }

        /// <summary>
        /// Consulta las cuentas bancarias del beneficiario para el banco seleccionado.
        /// VB6: txtIdentificación_LostFocus / cboBanco_Click -> exec spSys_Cuentas_Bancarias
        /// con @DivisaCheck = 1.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2DropdownDto>> Prea_frmPreaEstudiov2_Desembolsos_Cuentas_Consultar(
            int codEmpresa,
            string identificacion,
            string banco)
        {
            var response = new ErrorDto<List<FrmPreaEstudiov2DropdownDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                var identificacionNormalizada = (identificacion ?? string.Empty)
                    .Replace("undefined", string.Empty)
                    .Replace(" ", string.Empty)
                    .Trim();
                if (!int.TryParse((banco ?? string.Empty).Trim(), out var bancoId) || bancoId <= 0)
                {
                    response.Code = -2;
                    response.Description = "El banco es requerido.";
                    return response;
                }

                using var connection = _portalDb.CreateConnection(codEmpresa);
                const string sql = @"exec spSys_Cuentas_Bancarias @Identificacion, @BancoId, @DivisaCheck;";
                var rows = connection.Query(
                    sql,
                    new
                    {
                        Identificacion = identificacionNormalizada,
                        BancoId = bancoId,
                        DivisaCheck = 1
                    });

                var cuentas = new List<FrmPreaEstudiov2DropdownDto>();
                foreach (var row in rows)
                {
                    var dict = new Dictionary<string, object>((IDictionary<string, object>)row, StringComparer.OrdinalIgnoreCase);
                    var item = GetString(dict, "IdX").Trim();
                    if (item.Length == 0)
                    {
                        item = GetString(dict, "cuenta_interna").Trim();
                    }

                    if (item.Length == 0 || cuentas.Any(c => c.item == item))
                    {
                        continue;
                    }

                    var descripcion = GetString(dict, "ItmX").Trim();
                    if (descripcion.Length == 0)
                    {
                        descripcion = GetString(dict, "cuenta_desc").Trim();
                    }

                    cuentas.Add(new FrmPreaEstudiov2DropdownDto
                    {
                        item = item,
                        descripcion = descripcion.Length == 0 ? item : descripcion
                    });
                }

                response.Result = cuentas;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = [];
            }

            return response;
        }

        /// <summary>
        /// Guarda un desembolso del expediente. Fiel a VB6 sbDesembolso_Guardar
        /// (frmPreaEstudiov2.frm línea ~13148): exec spCrdPreaGuardaDesembolsos con 16
        /// parámetros posicionales, en el orden confirmado por el comentario de firma
        /// incluido en el propio código fuente VB6.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2DesembolsoGuardarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"EXEC spCrdPreaGuardaDesembolsos @Expediente, @CodAcreedor,
                    @Ordinario, @Descripcion, @Cuota, @Monto, @TipoGiro, @CedulaDestino,
                    @TipoCedula, @Cuenta, @CodDivisa, '', @Correo, @Detalle, '', @CodBanco";
                connection.Execute(sql, new
                {
                    Expediente = request.cod_preanalisis.Trim(),
                    CodAcreedor = (request.cod_acreedor ?? string.Empty).Trim(),
                    Ordinario = request.ordinario ? 1 : 0,
                    Descripcion = (request.descripcion ?? string.Empty).Trim(),
                    request.cuota,
                    request.monto,
                    TipoGiro = (request.tipo_giro ?? string.Empty).Trim(),
                    CedulaDestino = (request.cedula_destino ?? string.Empty).Trim(),
                    TipoCedula = request.tipo_cedula,
                    Cuenta = (request.cuenta ?? string.Empty).Trim(),
                    CodDivisa = (request.cod_divisa ?? string.Empty).Trim(),
                    Correo = (request.correo ?? string.Empty).Trim(),
                    Detalle = (request.detalle ?? string.Empty).Trim(),
                    CodBanco = (request.cod_banco ?? string.Empty).Trim()
                });

                return Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, request.cod_preanalisis, request.usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DesembolsosResponse();
                return response;
            }
        }

        /// <summary>
        /// Elimina un desembolso del expediente. Fiel a VB6 sbDesembolso_Borrar
        /// (frmPreaEstudiov2.frm línea ~13169): exec spCrdPreaEliminarDesembolsos '&lt;expediente&gt;', &lt;idX&gt;.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Eliminar(
            int codEmpresa,
            string cod_preanalisis,
            int id_desembolso,
            string usuario)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCrdPreaEliminarDesembolsos @Expediente, @IdDesembolso";
                connection.Execute(sql, new { Expediente = cod_preanalisis.Trim(), IdDesembolso = id_desembolso });

                return Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis, usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DesembolsosResponse();
                return response;
            }
        }
    }
}
