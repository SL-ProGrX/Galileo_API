using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXContabilidadesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 20;

        public FrmCntXContabilidadesDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCntXContabilidadesDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        public ErrorDto<CntXContabilidadData?> CntXContabilidad_Obtener(int codEmpresa, int codConta)
        {
            const string sql = @"exec spCntX_Contabilidad_Consulta @codConta;";

            return DbHelper.ExecuteSingleQuery<CntXContabilidadData>(
                _portalDb,
                codEmpresa,
                sql,
                new CntXContabilidadData(),
                new { codConta }
            );
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXContabilidades_Lista_Obtener(int codEmpresa)
        {
            const string query = @"select COD_CONTABILIDAD as item, nombre as descripcion from cntX_contabilidades";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<CntXContabilidadData?> CntXContabilidad_Scroll_Obtener(int codEmpresa, int scrollCode, int codConta)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
                    select Top 1 cod_contabilidad
                    from CntX_Contabilidades
                    WHERE
                          ((@scroll = 1 AND cod_contabilidad > @codConta)
                           OR (@scroll <> 1 AND cod_contabilidad < @codConta))
                    ORDER BY
                        CASE WHEN @scroll = 1 THEN cod_contabilidad END ASC,
                        CASE WHEN @scroll <> 1 THEN cod_contabilidad END DESC;";

                var codContabilidad = conn.Query<int>(query, new { scroll = scrollCode, codConta }).FirstOrDefault();
                var codContaObjetivo = (codContabilidad > 0) ? codContabilidad : codConta;

                return CntXContabilidad_Obtener(codEmpresa, codContaObjetivo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntXContabilidadData?>(ex.Message);
            }
        }

        public ErrorDto<List<DropDownConsolidaListaData>> CntXContabilidad_ConsolidaBaseList_Obtener(int codEmpresa, int codConta)
        {
            const string sql = @"exec spCntX_Consolida_Base_List @codConta;";

            return DbHelper.ExecuteListQuery<DropDownConsolidaListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { codConta }
            );
        }

        public ErrorDto<List<DropDownConsolidaListaData>> CntXContabilidad_ConsolidaUnidadesList_Obtener(int codEmpresa, int codConta)
        {
            const string sql = @"exec spCntX_Consolida_Unidades_List @codConta;";

            return DbHelper.ExecuteListQuery<DropDownConsolidaListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { codConta }
            );
        }

        public ErrorDto CntXContabilidades_Guardar(int codEmpresa, string usuario, bool edita, CntXContabilidadData request)
        {
            if (!edita)
            {
                var dup = ValidarNombreDuplicado(codEmpresa, request.nombre);
                if (dup.Code < 0)
                    return dup;
            }

            request.contabase_id = request.consolida_ind ? (request.contabase_id ?? 0) : 0;
            request.unidad_id = request.consolida_ind ? (request.unidad_id ?? string.Empty) : string.Empty;

            ErrorDto resp = edita
                ? ActualizarContabilidad(codEmpresa, usuario, request)
                : InsertarContabilidad(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto { Code = 0, Description = "Información guardada satisfactoriamente..." };
        }

        public ErrorDto CntXContabilidades_Eliminar(int codEmpresa, int codConta, string usuario)
        {
            const string sqlDelete = @"delete CntX_Contabilidades where cod_contabilidad = @CodConta;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { CodConta = codConta }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Contabilidad : {codConta}"
            );

            return resp;
        }

        #region helpers CntXContabilidades_Guardar 
        private ErrorDto ActualizarContabilidad(int codEmpresa, string usuario, CntXContabilidadData request)
        {
            var tieneCuentas = FxCatalogoLineas(codEmpresa, request.cod_contabilidad) > 0;

            var sqlUpdate = $@"
                update CntX_Contabilidades
                   set nombre = @Nombre,
                       cedula_juridica = @Cedula,
                       tel_fax = @TelFax,
                       tel_central = @TelCentral,
                       contacto = @Contacto,
                       email = @Email,
                       razonsocial = @RazonSocial,
                       hecho = '',
                       revisado = '',
                       expareas = @ExpAreas,
                       expasientos = @ExpAsientos,
                       expcuentas = @ExpCuentas,
                       expdiferidos = @ExpDiferidos,
                       expmantenimiento = @ExpMantenimiento,
                       expplanfijo = @ExpPlanFijo,
                       expplanrate = @ExpPlanRate,
                       exppresupuesto = @ExpPresupuesto,
                       filtra_ctas_bancos = @FiltraBancos,
                       filtra_ctas_contabilidad = @FiltraContabilidad,
                       filtra_ctas_inversiones = @FiltraInversiones,
                       filtra_ctas_operaciones = @FiltraOperaciones,
                       filtra_ctas_rrhh = @FiltraRRHH,
                       i_consolidadora = @ConsolidaInd,
                       consolida_conta_base = @ContaBase,
                       consolida_unidad_base = @UnidadBase
                       {(tieneCuentas ? "" : @",
                       nivel1 = @Nivel1, nivel2 = @Nivel2, nivel3 = @Nivel3, nivel4 = @Nivel4,
                       nivel5 = @Nivel5, nivel6 = @Nivel6, nivel7 = @Nivel7, nivel8 = @Nivel8")}
                       ,
                       modifica_fecha = getdate(),
                       modifica_usuario = @Usuario
                 where cod_contabilidad = @CodConta;";

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlUpdate, ParametrosGuardar(request, usuario));

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Contabilidad : {request.cod_contabilidad}");
            return resp;
        }

        private ErrorDto InsertarContabilidad(int codEmpresa, string usuario, CntXContabilidadData request)
        {
            request.cod_contabilidad = ObtenerConsecutivoConta(codEmpresa);

            const string sqlInsert = @"
                insert into CntX_Contabilidades(
                    cod_contabilidad,nombre,cedula_juridica,tel_fax,tel_central,contacto,email,
                    razonsocial,nivel1,nivel2,nivel3,nivel4,nivel5,nivel6,nivel7,nivel8,hecho,revisado,
                    expareas,expcuentas,expasientos,expmantenimiento,expdiferidos,
                    expplanfijo,expplanrate,exppresupuesto,
                    filtra_ctas_bancos,filtra_ctas_contabilidad,filtra_ctas_inversiones,filtra_ctas_operaciones,filtra_ctas_rrhh,
                    i_consolidadora,consolida_conta_base,consolida_unidad_base,
                    registro_fecha,registro_usuario
                )
                values(
                    @CodConta,@Nombre,@Cedula,@TelFax,@TelCentral,@Contacto,@Email,
                    @RazonSocial,@Nivel1,@Nivel2,@Nivel3,@Nivel4,@Nivel5,@Nivel6,@Nivel7,@Nivel8,'','',
                    @ExpAreas,@ExpCuentas,@ExpAsientos,@ExpMantenimiento,@ExpDiferidos,
                    @ExpPlanFijo,@ExpPlanRate,@ExpPresupuesto,
                    @FiltraBancos,@FiltraContabilidad,@FiltraInversiones,@FiltraOperaciones,@FiltraRRHH,
                    @ConsolidaInd,@ContaBase,@UnidadBase,
                    getdate(),@Usuario
                );";

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlInsert, ParametrosGuardar(request, usuario));

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Contabilidad : {request.cod_contabilidad}");

            if (request.crear_predeterminados)
            {
                var respPred = PredeterminarConfiguracion(codEmpresa, request.cod_contabilidad, usuario);
                if (respPred.Code < 0)
                    return respPred;
            }

            return resp;
        }

        private static object ParametrosGuardar(CntXContabilidadData r, string usuario) => new
        {
            CodConta = r.cod_contabilidad,
            Nombre = r.nombre ?? "",
            Cedula = r.cedula_juridica ?? "",
            TelFax = r.tel_fax ?? "",
            TelCentral = r.tel_central ?? "",
            Contacto = r.contacto ?? "",
            Email = r.email ?? "",
            RazonSocial = r.razonsocial ?? "C",

            ExpAreas = (r.expareas ?? false) ? 1 : 0,
            ExpAsientos = (r.expasientos ?? false) ? 1 : 0,
            ExpCuentas = (r.expcuentas ?? false) ? 1 : 0,
            ExpDiferidos = (r.expdiferidos ?? false) ? 1 : 0,
            ExpMantenimiento = (r.expmantenimiento ?? false) ? 1 : 0,
            ExpPlanFijo = (r.expplanfijo ?? false) ? 1 : 0,
            ExpPlanRate = (r.expplanrate ?? false) ? 1 : 0,
            ExpPresupuesto = (r.exppresupuesto ?? false) ? 1 : 0,

            FiltraBancos = (r.filtra_ctas_bancos ?? false) ? 1 : 0,
            FiltraContabilidad = (r.filtra_ctas_contabilidad ?? false) ? 1 : 0,
            FiltraInversiones = (r.filtra_ctas_inversiones ?? false) ? 1 : 0,
            FiltraOperaciones = (r.filtra_ctas_operaciones ?? false) ? 1 : 0,
            FiltraRRHH = (r.filtra_ctas_rrhh ?? false) ? 1 : 0,

            ConsolidaInd = r.consolida_ind ? 1 : 0,
            ContaBase = r.contabase_id ?? 0,
            UnidadBase = r.unidad_id ?? "",

            Nivel1 = r.nivel1 ?? 0,
            Nivel2 = r.nivel2 ?? 0,
            Nivel3 = r.nivel3 ?? 0,
            Nivel4 = r.nivel4 ?? 0,
            Nivel5 = r.nivel5 ?? 0,
            Nivel6 = r.nivel6 ?? 0,
            Nivel7 = r.nivel7 ?? 0,
            Nivel8 = r.nivel8 ?? 0,

            Usuario = usuario
        };

        private ErrorDto PredeterminarConfiguracion(int codEmpresa, int codConta, string usuario)
        {
            const string sql = @"exec spCntX_Util_Contabilidad_Cfg_Predetermina @CodConta, 1, @Usuario, '*xHM1tOk3n$';";
            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new { CodConta = codConta, Usuario = usuario });
        }

        private ErrorDto ValidarNombreDuplicado(int codEmpresa, string nombre)
        {
            const string sql = @"select isnull(count(*),0) as Total
                                 from CntX_Contabilidades
                                 where upper(nombre) = upper(@Nombre);";

            var result = DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Nombre = nombre.Trim() }
            );

            if (result.Code < 0)
                return new ErrorDto { Code = result.Code, Description = result.Description };

            int total = result.Result;

            if (total > 0)
                return new ErrorDto { Code = -2, Description = "El nombre de esta contabilidad ya se encuentra registrado verifique..." };

            return new ErrorDto { Code = 0, Description = "Ok" };
        }

        private int FxCatalogoLineas(int codEmpresa, int codConta)
        {
            const string sql = @"select count(*) as Lineas from CntX_Cuentas where cod_contabilidad = @CodConta;";

            var result = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { CodConta = codConta }
            );

            if (result.Code < 0)
                return 0;

            try { return result.Result; } catch { return 0; }
        }

        private int ObtenerConsecutivoConta(int codEmpresa)
        {
            const string sql = @"select isnull(max(cod_contabilidad),0) + 1 as ultimo from CntX_Contabilidades;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { }
            ).Result;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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
