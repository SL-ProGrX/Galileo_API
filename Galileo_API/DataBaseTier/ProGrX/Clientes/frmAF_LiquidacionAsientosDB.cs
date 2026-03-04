using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfLiquidacionAsientosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MTesoreria _mtes;
        private readonly MProGrxMain _main;

        public FrmAfLiquidacionAsientosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mtes = new MTesoreria(config);
            _main = new MProGrxMain(config);
        }

        public ErrorDto<string> fxSIFParametros(int CodEmpresa, string codigo)
        {
            var valor = _main.FxSIFParametros(CodEmpresa, codigo);
            return new ErrorDto<string>
            {
                Result = valor,
                Code = 0,
                Description = "OK"
            };
        }

        /// <summary>
        /// Metodo: Obtiene los bancos disponibles para la liquidación de afiliaciones, filtrados por fecha de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Bancos(
                int CodEmpresa,
                AfLiquidacionFiltroRequest request)
                    {
                        return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                        {
                            var query = @"
                                Select L.cod_banco as item,
                                       isnull(B.descripcion,'Sin Banco') as descripcion
                                From Liquidacion L
                                left join Tes_Bancos B on L.cod_Banco = B.id_Banco
                                Where L.FecLiq between @Desde and @Hasta
                                Group by L.cod_banco,B.descripcion";

                            var response = conn.Query<DropDownListaGenericaModel>(query, new
                            {
                                Desde = request.desde,
                                Hasta = request.hasta
                            }).ToList();

                            //agregar opcion TODOS al inicio
                            response.Insert(0, new DropDownListaGenericaModel
                            {
                                item = "T",
                                descripcion = ConstanteLiquidacionAsientos.todos
                            });

                            return response;
                        });
        }

        /// <summary>
        /// Metodo: Obtiene los usuarios disponibles para la liquidación de afiliaciones, filtrados por fecha de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Usuarios(
                int CodEmpresa,
                AfLiquidacionFiltroRequest request)
                    {
                        return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                        {
                            var query = @"
            Select L.USUARIO as item,
                   L.USUARIO as descripcion
            From Liquidacion L
            Where L.FecLiq between @Desde and @Hasta
            Group by L.USUARIO";

                            var response = conn.Query<DropDownListaGenericaModel>(query, new
                            {
                                Desde = request.desde,
                                Hasta = request.hasta
                            }).ToList();

                            //agregar opcion TODOS al inicio
                            response.Insert(0, new DropDownListaGenericaModel
                            {
                                item = "T",
                                descripcion = ConstanteLiquidacionAsientos.todos
                            });

                            return response;
                        });
        }

        /// <summary>
        /// Metodo: Obtiene los tokens disponibles para la liquidación de afiliaciones, filtrados por fecha de liquidación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Tokens(
                int CodEmpresa,
                AfLiquidacionFiltroRequest request)
                    {
                        return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                        {
                            var query = @"
                                Select ISNULL(L.ID_TOKEN,'') as item,
                                       ISNULL(L.ID_TOKEN,'') as descripcion
                                From Liquidacion L
                                Where L.FecLiq between @Desde and @Hasta
                                Group by ISNULL(L.ID_TOKEN,'')";

                            var response = conn.Query<DropDownListaGenericaModel>(query, new
                            {
                                Desde = request.desde,
                                Hasta = request.hasta
                            }).ToList();

                            //agregar opcion TODOS al inicio
                            response.Insert(0, new DropDownListaGenericaModel
                            {
                                item = "T",
                                descripcion = ConstanteLiquidacionAsientos.todos
                            });

                            return response;
                        });
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Af_LiquidacionAsientos_Oficinas(
            int CodEmpresa,
            AfLiquidacionFiltroRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = @"
                    Select rtrim(L.cod_Oficina) as item,
                           isnull(O.descripcion,'') as descripcion
                    From Liquidacion L
                    left join SIF_Oficinas O on L.cod_oficina = O.cod_oficina
                    Where L.FecLiq between @Desde and @Hasta
                    Group by L.cod_Oficina,O.descripcion";

                var response = conn.Query<DropDownListaGenericaModel>(query, new
                {
                    Desde = request.desde,
                    Hasta = request.hasta
                }).ToList();

                //agregar opcion TODOS al inicio
                response.Insert(0, new DropDownListaGenericaModel
                {
                    item = "T",
                    descripcion = ConstanteLiquidacionAsientos.todos
                });

                return response;
            });
        }

        /// <summary>
        /// Método: Obtiene los tipos de asiento para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqAsientosTipo_Obtener(int CodEmpresa, string accion)
        {
            string query = accion switch
            {
                "D" => @"
                    select id_banco as Item, rtrim(descripcion) + '  ' + rtrim(Cta) as descripcion
                    from Tes_Bancos
                    where estado = 'A'",
                "R" => @"
                    select RTRIM(RETENCION_CODIGO) as Item,
                           RTRIM(RETENCION_CODIGO) + ' - ' + rtrim(descripcion) + ' [' + rtrim(COD_CUENTA) + ']' as descripcion
                    from FND_RETENCION_CONCEPTOS
                    where ACTIVO = 1",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
                return DbHelper.CreateErrorResponse("Acción no válida para obtener tipos de asiento.", -1, new List<DropDownListaGenericaModel>());

            var response = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, CodEmpresa, query);

            //agregar opcion TODOS al inicio
            response.Result.Insert(0, new DropDownListaGenericaModel
            {
                item = "T",
                descripcion = ConstanteLiquidacionAsientos.todos
            });

            return response;
        }

        /// <summary>
        /// Método: Obtiene los tokens disponibles para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<TokenConsultaModel>> AF_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_Consulta(CodEmpresa, usuario);
        }

        /// <summary>
        /// Método: Genera un nuevo token para la liquidación de afiliaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _mtes.spTes_Token_New(CodEmpresa, usuario);
        }


        /// <summary>
        /// OBJETIVO:      Genera a Tesoreria las liquidaciones.
        /// 'REFERENCIAS:   AsientoLiquidacionTesoreria - (Genera el Asiento de la liquidacion en el
        /// '               modulo de Tesoreria)
        /// '               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        /// '               Procedimiento)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="token"></param>
        /// <param name="usuario"></param>
        /// <param name="liquidaciones"></param>
        /// <returns></returns>
        public ErrorDto<AfLiquidacionAsientosGenerarResponse> Af_LiquidacionAsientos_Generar(
              int CodEmpresa,
              AfLiquidacionAsientosGenerarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            #region Validaciones

            if (request.accion == "D" && string.IsNullOrWhiteSpace(request.token))
                return DbHelper.CreateErrorResponse<AfLiquidacionAsientosGenerarResponse>("token es requerido para Desembolsar (D).");

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.CreateErrorResponse<AfLiquidacionAsientosGenerarResponse>("usuario es requerido.");

            if (request.items == null || request.items.Count == 0)
                return DbHelper.CreateErrorResponse<AfLiquidacionAsientosGenerarResponse>("No hay items para procesar.");

            #endregion

            var resp = new AfLiquidacionAsientosGenerarResponse
            {
                total_items = request.items.Count,
                seleccionados = request.items.Count(x => x.marcado),
                omitidos_no_marcado = request.items.Count(x => !x.marcado),
                omitidos_duplicado = request.items.Count(x => x.marcado && x.duplicado == 1),
                procesados = 0
            };

            try
            {
                foreach (var it in request.items)
                {
                    if (!it.marcado) continue;
                    if (it.duplicado == 1) continue;

                    if (request.accion == "D")
                    {
                        // VB6: exec spAFI_Liquidacion_Traslado_Bancos <consec>, '<usuario>', '<token>'
                        const string sp = "EXEC spAFI_Liquidacion_Traslado_Bancos @Liq, @Usuario, @Token;";
                        conn.Execute(sp, new
                        {
                            Liq = it.consec,
                            Usuario = request.usuario.Trim(),
                            Token = request.token.Trim()
                        });
                    }
                    else
                    {
                        // VB6: Retener (UPDATE Liquidacion ...)
                        const string sql = @"
                                UPDATE Liquidacion
                                SET Fecha_Traspaso = dbo.MyGetdate(),
                                    EstadoAsiento = 'G',
                                    NDocumento = '0',
                                    Tdocumento = 'RT',
                                    Tesoreria_Solicitud = 0,
                                    Traspaso_Usuario = @Usuario
                                WHERE Consec = @Consec;";

                        conn.Execute(sql, new
                        {
                            Usuario = request.usuario.Trim(),
                            Consec = it.consec
                        });
                    }

                    resp.procesados++;
                }

                return DbHelper.CreateOkResponse<AfLiquidacionAsientosGenerarResponse>(resp);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<AfLiquidacionAsientosGenerarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// OBJETIVO:
        ///     Ejecuta la consulta de liquidaciones para traslado a Tesorería,
        ///     equivalente al método sbBuscar del formulario VB6 frmAF_LiquidacionAsientos.
        ///
        /// DESCRIPCIÓN FUNCIONAL (VB6 original):
        ///     - Consulta liquidaciones dentro de un rango de fechas.
        ///     - Permite filtrar por:
        ///         * Tipo de salida (Banco cuando Acción = Desembolsar)
        ///         * Estado del asiento (Pendiente / Generado)
        ///         * Tipo de renuncia (Asociación / Patronal)
        ///         * Filtros adicionales: Banco, Oficina, Usuario, Token
        ///     - Ejecuta el SP:
        ///         spAFI_Liquidacion_Traslado_Bancos_Lista
        ///
        /// PARAMETROS IMPORTANTES:
        ///     @Marcar            -> 1/0 (equivalente a chkTodos.Value)
        ///     @Desde             -> Fecha inicio 00:00:00
        ///     @Hasta             -> Fecha fin 23:59:59
        ///     @TipoSalida        -> Id Banco (solo si Acción = Desembolsar)
        ///     @EstadoAsiento     -> 'P' o 'G'
        ///     @TipoRenuncia      -> 'A', 'P' o NULL
        ///     @Banco             -> Filtro adicional banco
        ///     @Oficina           -> Filtro adicional oficina
        ///     @Usuario           -> Filtro adicional usuario
        ///     @Token             -> Filtro adicional token
        ///
        /// OBSERVACIONES:
        ///     - Valida rango de fechas.
        ///     - Normaliza valores tipo char a su primera letra (P/G/A).
        ///     - Devuelve listado tipado para el grid.
        /// </summary>
        public ErrorDto<List<AfLiquidacionAsientosRowDto>> Af_LiquidacionAsientos_Buscar(
            int CodEmpresa,
            AfLiquidacionAsientosBuscarRequest request)
        {
            #region Validaciones

            if (request == null)
                return DbHelper.CreateErrorResponse<List<AfLiquidacionAsientosRowDto>>("Request inválido.");

            if (request.desde == default || request.hasta == default)
                return DbHelper.CreateErrorResponse<List<AfLiquidacionAsientosRowDto>>("El rango de fechas es requerido.");

            if (request.hasta.Date < request.desde.Date)
                return DbHelper.CreateErrorResponse<List<AfLiquidacionAsientosRowDto>>("Verifique el rango de fechas.");

            #endregion

            #region Normalización de Fechas (00:00:00 - 23:59:59)

            var desde = request.desde.Date;
            var hasta = request.hasta.Date.AddDays(1).AddTicks(-1);

            #endregion

            #region Normalización de Flags Tipo Char

            string? estadoAsiento = NormalizeChar(request.estado_asiento, new[] { "P", "G" });
            string? tipoRenuncia = NormalizeChar(request.tipo_renuncia, new[] { "A", "P" });

            #endregion

            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                    EXEC spAFI_Liquidacion_Traslado_Bancos_Lista
                        @SelValue,
                        @Inicio,
                        @Corte,
                        @TipoSalida,
                        @EstadoAsiento,
                        @TipoRenuncia,
                        @BancoId,
                        @Oficina,
                        @Usuario,
                        @Token;";

                var param = new
                {
                    SelValue = request.marcar ? 1 : 0,
                    Inicio = desde,
                    Corte = hasta,
                    TipoSalida = request.tipo_salida,
                    EstadoAsiento = estadoAsiento,
                    TipoRenuncia = tipoRenuncia,
                    BancoId = request.filtro_banco,
                    Oficina = string.IsNullOrWhiteSpace(request.filtro_oficina) ? null : request.filtro_oficina.Trim(),
                    Usuario = string.IsNullOrWhiteSpace(request.filtro_usuario) ? null : request.filtro_usuario.Trim(),
                    Token = string.IsNullOrWhiteSpace(request.filtro_token) ? null : request.filtro_token.Trim()
                };

                return conn.Query<AfLiquidacionAsientosRowDto>(query, param).ToList();
            });
        }

        /// <summary>
        /// Normaliza valores tipo catálogo que en VB6 se evaluaban por la primera letra.
        /// Ejemplo:
        ///     "Pendiente" -> "P"
        ///     "Generado"  -> "G"
        ///     "Asociación"-> "A"
        /// </summary>
        private static string? NormalizeChar(string? input, string[] allowed)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var x = input.Trim();

            if (x.Length > 1)
                x = x.Substring(0, 1);

            x = x.ToUpperInvariant();

            return allowed.Contains(x) ? x : null;
        }

    }
}