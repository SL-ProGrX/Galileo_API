using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDiferidosPlantillaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;
        private readonly int vModulo = 20;

        public FrmCntXDiferidosPlantillaDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config)
            )
        { }

        public FrmCntXDiferidosPlantillaDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMain = mProGrxMain;
        }

        public ErrorDto<CntXDiferidosData?> CntXDiferidosPlantilla_Obtener(int codEmpresa, int codConta, int codDiferido)
        {
            const string query = @"select * from CntX_Diferidos where cod_contabilidad = @codConta 
                and cod_diferido = @codDiferido";

            return DbHelper.ExecuteSingleQuery(_portalDb, codEmpresa, query, new CntXDiferidosData(), new { codConta, codDiferido });
        }

        public ErrorDto<CntXDiferidosData?> CntXDiferidosPlantilla_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, int codDiferido)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            try
            {
                const string query = @"
                    SELECT TOP 1 cod_diferido 
                    FROM CntX_Diferidos 
                    WHERE cod_contabilidad = @codConta AND 
                          ((@scroll = 1 AND cod_diferido > @codDiferido)
                           OR (@scroll <> 1 AND cod_diferido < @codDiferido))
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN cod_diferido END ASC,
                        CASE WHEN @scroll <> 1 THEN cod_diferido END DESC;";

                var diferido = conn.Query<int?>(query, 
                    new { scroll = scrollCode,  codConta, codDiferido })
                    .FirstOrDefault();

                var diferidoObjetivo = diferido.HasValue
                    ? diferido.Value : codDiferido;

                return CntXDiferidosPlantilla_Obtener(CodEmpresa, codConta, diferidoObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntXDiferidosData?>(ex.Message);
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDiferidosPlantilla_Lista_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select cod_diferido as item,descripcion from CntX_Diferidos 
                where cod_contabilidad = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDiferidosPlantilla_TiposAsientos_Obtener(int codEmpresa, int codConta)
        {
            const string query = @"select Tipo_Asiento as item,descripcion from CntX_Tipos_Asientos 
                where cod_contabilidad = @codConta";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query, new { codConta });
        }

        public ErrorDto<string?> CntXDiferidosPlantilla_TipoAsientoDesc_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            const string query = @"select descripcion from CntX_Tipos_Asientos 
                where cod_contabilidad = @codConta and tipo_asiento = @tipoAsiento";

            return DbHelper.ExecuteSingleQuery(_portalDb, codEmpresa, query, tipoAsiento, new { codConta, tipoAsiento });
        }

        public ErrorDto<List<CntXDiferidosDetalleData>> CntXDiferidosPlantilla_Detalle_Obtener(int codEmpresa, int codConta, int codDiferido)
        {
            const string query = @"select A.cod_cuenta,B.descripcion,porc_debito,porc_credito,linea,
                A.cod_unidad,U.descripcion as UniDes,A.cod_divisa,A.cod_centro_costo 
                from CntX_Diferidos_detalle A inner join CntX_Cuentas B 
                on A.cod_cuenta = B.cod_cuenta and A.cod_contabilidad = B.cod_contabilidad 
                inner join CntX_Unidades U on A.cod_unidad = U.cod_unidad and A.cod_contabilidad = U.cod_contabilidad 
                left join CntX_Centro_Costos C on A.cod_centro_costo = C.cod_centro_costo and A.cod_contabilidad = C.cod_contabilidad
                where A.cod_contabilidad = @codConta 
                and A.cod_diferido = @codDiferido 
                order by linea";

            return DbHelper.ExecuteListQuery<CntXDiferidosDetalleData>(_portalDb, codEmpresa, query, new { codConta, codDiferido });
        }

        public ErrorDto CntXDiferidosPlantilla_Guardar(int codEmpresa, CntXDiferidosPlantillaRequest request)
        {
            var vr = ValidarRequestYAsiento(codEmpresa, request);
            if (vr.Code < 0) return vr;

            int codConta = request.cod_contabilidad;
            string usuario = (request.usuario ?? string.Empty).Trim();
            var data = request.data;

            var n = NormalizarCabecera(request);

            try
            {
                int codDiferido = GuardarEncabezado(codEmpresa, codConta, usuario, request, data, n);

                var respDet = ReemplazarDetalle(codEmpresa, codConta, codDiferido, n.TipoAsiento, request.detalles);
                if (respDet.Code < 0) return respDet;

                return RespuestaOk(request.edita, codDiferido);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        public ErrorDto CntXDiferidosPlantilla_Eliminar(int codEmpresa, int codConta, string usuario, int codDiferido)
        {
            const string sqlDeleteDetalle = @"delete CntX_Diferidos_detalle 
                where cod_contabilidad = @CodConta and cod_diferido = @CodDiferido;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeleteDetalle,
                new { CodConta = codConta, CodDiferido = codDiferido }
            );

            if (resp.Code < 0)
                return resp;

            const string sqlDelete = @"delete CntX_Diferidos 
                where cod_contabilidad = @CodConta and cod_diferido = @CodDiferido;";

            resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { CodConta = codConta, CodDiferido = codDiferido }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Plantilla Diferidos : {codDiferido} Conta.{codConta}"
            );

            return resp;
        }

        #region helpers CntXDiferidosPlantilla_Guardar 

        private ErrorDto ValidarRequestYAsiento(int codEmpresa, CntXDiferidosPlantillaRequest request)
        {
            if (request == null || request.data == null)
                return new ErrorDto { Code = -2, Description = "La información especificada no es válida, verifíquela..." };

            decimal diferenciaPct = 0m;
            decimal debitoPct = 100m;
            decimal creditoPct = 100m;

            return FxVerificaAsiento(codEmpresa, request, diferenciaPct, debitoPct, creditoPct);
        }

        private sealed record Normalizado(string Descripcion, string TipoAsiento, string Observacion, string Tipo, int AsientoResumen);

        private static Normalizado NormalizarCabecera(CntXDiferidosPlantillaRequest request)
        {
            var data = request.data;

            string descripcion = (data.descripcion ?? string.Empty).Trim().ToUpperInvariant();
            string tipoAsiento = (data.tipo_asiento ?? string.Empty).Trim().ToUpperInvariant();
            string observacion = (data.observacion ?? string.Empty).Trim();

            string tipo = (data.tipo ?? string.Empty).Trim();
            if (tipo.Length > 1) tipo = tipo.Substring(0, 1);

            int asientoResumen = request.asiento_plantilla ? 1 : 0;

            return new Normalizado(descripcion, tipoAsiento, observacion, tipo, asientoResumen);
        }

        private int GuardarEncabezado(
            int codEmpresa, int codConta, string usuario,
            CntXDiferidosPlantillaRequest request,
            CntXDiferidosData data,
            Normalizado n)
        {
            int codDiferido = data.cod_diferido;

            if (request.edita)
            {
                if (codDiferido <= 0)
                    throw new ArgumentException("Código de plantilla inválido para modificar.");

                var resp = ActualizarEncabezado(codEmpresa, codConta, codDiferido, n);
                if (resp?.Code < 0) throw new InvalidOperationException(resp.Description);

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    movimiento: "Modifica - WEB",
                    detalle: $"Plantilla Diferido : {codDiferido} Conta.{codConta}"
                );

                return codDiferido;
            }

            codDiferido = ObtenerSiguienteCodDiferido(codEmpresa, codConta);

            var respInsert = InsertarEncabezado(codEmpresa, codConta, usuario, codDiferido, n);
            if (respInsert?.Code < 0) throw new InvalidOperationException(respInsert.Description);

            data.cod_diferido = codDiferido;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Plantilla Diferido : {codDiferido} Conta.{codConta}"
            );

            return codDiferido;
        }

        private ErrorDto ActualizarEncabezado(int codEmpresa, int codConta, int codDiferido, Normalizado n)
        {
            const string sqlUpdate = @"
                update CntX_Diferidos set
                    descripcion = @Descripcion,
                    tipo_asiento = @TipoAsiento,
                    observacion = @Observacion,
                    tipo = @Tipo,
                    ASIENTO_RESUMEN = @AsientoResumen
                where cod_contabilidad = @CodConta
                  and cod_diferido = @CodDiferido;";

            return DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sqlUpdate,
                new
                {
                    Descripcion = n.Descripcion,
                    TipoAsiento = n.TipoAsiento,
                    Observacion = n.Observacion,
                    Tipo = n.Tipo,
                    AsientoResumen = n.AsientoResumen,
                    CodConta = codConta,
                    CodDiferido = codDiferido
                }
            );
        }

        private int ObtenerSiguienteCodDiferido(int codEmpresa, int codConta)
        {
            const string sqlNext = @"
                select isnull(max(cod_diferido),0) + 1
                from CntX_Diferidos
                where cod_contabilidad = @CodConta;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb, codEmpresa, sqlNext, 0,
                new { CodConta = codConta }
            ).Result;
        }

        private ErrorDto InsertarEncabezado(int codEmpresa, int codConta, string usuario, int codDiferido, Normalizado n)
        {
            const string sqlInsert = @"
                insert into CntX_Diferidos
                (cod_diferido, tipo_asiento, cod_contabilidad,
                 descripcion, consecutivo, observacion,
                 user_crea, fecha_crea, tipo, ASIENTO_RESUMEN)
                values
                (@CodDiferido, @TipoAsiento, @CodConta,
                 @Descripcion, 0, @Observacion,
                 @Usuario, getdate(), @Tipo, @AsientoResumen);";

            return DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sqlInsert,
                new
                {
                    CodDiferido = codDiferido,
                    TipoAsiento = n.TipoAsiento,
                    CodConta = codConta,
                    Descripcion = n.Descripcion,
                    Observacion = n.Observacion,
                    Usuario = usuario,
                    Tipo = n.Tipo,
                    AsientoResumen = n.AsientoResumen
                }
            );
        }

        private ErrorDto ReemplazarDetalle(
            int codEmpresa,
            int codConta,
            int codDiferido,
            string tipoAsiento,
            List<CntXDiferidosDetalleData> detalles)
        {
            var respDel = BorrarDetalle(codEmpresa, codConta, codDiferido);
            if (respDel?.Code < 0) return respDel;

            return InsertarDetalle(codEmpresa, codConta, codDiferido, tipoAsiento, detalles);
        }

        private ErrorDto BorrarDetalle(int codEmpresa, int codConta, int codDiferido)
        {
            const string sqlDeleteDetalle = @"
                delete from CntX_Diferidos_detalle
                where cod_contabilidad = @CodConta
                  and cod_diferido = @CodDiferido;";

            return DbHelper.ExecuteNonQuery(
                _portalDb, codEmpresa, sqlDeleteDetalle,
                new { CodConta = codConta, CodDiferido = codDiferido }
            );
        }

        private ErrorDto InsertarDetalle(
            int codEmpresa, int codConta,
            int codDiferido, string tipoAsiento,
            List<CntXDiferidosDetalleData> detalles)
        {
            const string sqlInsertDetalle = @"
                insert into CntX_Diferidos_detalle
                (cod_diferido, cod_contabilidad, linea,
                 cod_cuenta, cod_unidad, cod_centro_costo,
                 cod_divisa, porc_debito, porc_credito, tipo_asiento)
                values
                (@CodDiferido, @CodConta, @Linea,
                 @CodCuenta, @CodUnidad, @CodCentroCosto,
                 @CodDivisa, @PorcDebito, @PorcCredito, @TipoAsiento);";

            int linea = 1;

            foreach (var d in (detalles ?? new List<CntXDiferidosDetalleData>())
                .Where(x => !string.IsNullOrWhiteSpace(x.cod_cuenta)))
            {
                var respDet = DbHelper.ExecuteNonQuery(
                    _portalDb, codEmpresa, sqlInsertDetalle,
                    new
                    {
                        CodDiferido = codDiferido,
                        CodConta = codConta,
                        Linea = linea++,
                        CodCuenta = NormalizarCuenta(d.cod_cuenta),
                        CodUnidad = (d.cod_unidad ?? string.Empty).Trim(),
                        CodCentroCosto = (d.cod_centro_costo ?? string.Empty).Trim(),
                        CodDivisa = (d.cod_divisa ?? string.Empty).Trim(),
                        PorcDebito = d.porc_debito,
                        PorcCredito = d.porc_credito,
                        TipoAsiento = tipoAsiento
                    }
                );

                if (respDet?.Code < 0) return respDet;
            }

            return new ErrorDto { Code = 0, Description = "OK" };
        }

        private static string NormalizarCuenta(string cuenta)
        {
            return (cuenta ?? string.Empty).Replace("-", "").Trim();
        }

        private static ErrorDto RespuestaOk(bool edita, int codDiferido)
        {
            return new ErrorDto
            {
                Code = 0,
                Description = edita
                    ? "Plantilla diferido actualizada satisfactoriamente."
                    : $"Plantilla diferido registrada satisfactoriamente. Código: {codDiferido}"
            };
        }

        private ErrorDto FxVerificaAsiento(
            int codEmpresa, CntXDiferidosPlantillaRequest request,
            decimal diferenciaPct, decimal debitoPct, decimal creditoPct)
        {
            if (request == null || request.data == null)
                return new ErrorDto { Code = -2, Description = "La información especificada no es válida, verifíquela..." };

            int codConta = request.cod_contabilidad;
            string tipoAsiento = (request.data.tipo_asiento ?? string.Empty).Trim();

            var errores = new List<string>();

            const string sqlExisteTipo = @"
                select isnull(count(*),0)
                from CntX_Tipos_Asientos
                where cod_contabilidad = @CodConta
                  and tipo_asiento = @TipoAsiento;";

            int existeTipo = DbHelper.ExecuteSingleQuery(
                _portalDb, codEmpresa, sqlExisteTipo, 0,
                new { CodConta = codConta, TipoAsiento = tipoAsiento }
            ).Result;

            if (existeTipo == 0)
                errores.Add("- El tipo de Asiento indicado no existe...");

            if (diferenciaPct != 0m)
                errores.Add("- El Asiento no se encuentra Balanceado...");

            if (debitoPct != 100m)
                errores.Add("- Los débitos no estan al 100%");

            if (creditoPct != 100m)
                errores.Add("- Los créditos no estan al 100%");

            var cuentas = (request.detalles ?? new List<CntXDiferidosDetalleData>())
                .Where(d => !string.IsNullOrWhiteSpace(d.cod_cuenta))
                .Select(d => d.cod_cuenta.Replace("-", "").Trim()) 
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cuentas.Count > 0)
            {
                const string sqlCuentasValidas = @"
                    select cod_cuenta
                    from CntX_Cuentas
                    where cod_contabilidad = @CodConta
                      and acepta_movimientos = 1
                      and cod_cuenta in @Cuentas;";

                var existentesDto = DbHelper.ExecuteListQuery<string>(
                    _portalDb, codEmpresa, sqlCuentasValidas,
                    new { CodConta = codConta, Cuentas = cuentas }
                );

                var existentes = existentesDto?.Result ?? new List<string>();
                var setExistentes = new HashSet<string>(existentes, StringComparer.OrdinalIgnoreCase);

                foreach (var cta in cuentas)
                {
                    if (!setExistentes.Contains(cta))
                        errores.Add($"- Cuenta {cta} No Existe");
                }
            }

            if (errores.Count > 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = string.Join("\n", errores)
                };
            }

            return new ErrorDto { Code = 0, Description = "OK" };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        #endregion
    }
}
