using Galileo.Models.ERROR;
using Galileo.DataBaseTier;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrTasasPtsBonificacionDb
    {
        private const string MovimientoRegistraWeb = "Registra - WEB";
        private const string MovimientoModificaWeb = "Modifica - WEB";
        private const string MovimientoEliminaWeb = "Elimina - WEB";

        private sealed class BitacoraLineaInfo
        {
            public string usuario { get; set; } = string.Empty;
            public string cod_tasa_bono { get; set; } = string.Empty;
            public int linea { get; set; }
            public string movimiento { get; set; } = string.Empty;
            public string detalle_base { get; set; } = string.Empty;
        }

        private ErrorDto InsertarPlan(int codEmpresa, CrTasasPtsBonificacionDefinicionGuardarRequest request)
        {
            const string sql = @"
                insert into CRD_TASA_BONO
                (
                    cod_Tasa_Bono,
                    descripcion,
                    Notas,
                    Activo,
                    Registro_Fecha,
                    Registro_Usuario
                )
                values
                (
                    @CodTasaBono,
                    @Descripcion,
                    @Notas,
                    @Activo,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.definicion.cod_tasa_bono,
                    Descripcion = request.definicion.descripcion,
                    Notas = request.definicion.notas,
                    Activo = request.definicion.activo ? 1 : 0,
                    Usuario = request.usuario
                });

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(codEmpresa, request.usuario, MovimientoRegistraWeb, $"Tasa: Plan de Bonificacion : {request.definicion.cod_tasa_bono}");
            return new ErrorDto { Code = 0, Description = GuardadoExitoso };
        }

        private ErrorDto ActualizarPlan(int codEmpresa, CrTasasPtsBonificacionDefinicionGuardarRequest request)
        {
            const string sql = @"
                update CRD_TASA_BONO
                   set descripcion = @Descripcion,
                       Notas = @Notas,
                       Activo = @Activo
                 where cod_Tasa_Bono = @CodTasaBono;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.definicion.cod_tasa_bono,
                    Descripcion = request.definicion.descripcion,
                    Notas = request.definicion.notas,
                    Activo = request.definicion.activo ? 1 : 0
                });

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(codEmpresa, request.usuario, MovimientoModificaWeb, $"Tasa: Plan de Bonificacion : {request.codigo_original}");
            return new ErrorDto { Code = 0, Description = GuardadoExitoso };
        }

        private ErrorDto InsertarMembresia(int codEmpresa, CrTasasPtsBonificacionMembresiaGuardarRequest request)
        {
            int nuevaLinea = ObtenerSiguienteLinea(codEmpresa, "CRD_TASA_BONO_MEMBRESIA", request.cod_tasa_bono);

            const string sql = @"
                insert into CRD_TASA_BONO_MEMBRESIA
                (
                    COD_TASA_BONO,
                    Linea,
                    Inicio,
                    Corte,
                    Tasa_Bono,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @CodTasaBono,
                    @Linea,
                    @Inicio,
                    @Corte,
                    @TasaBono,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            return EjecutarGuardarLinea(
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = nuevaLinea,
                    Inicio = request.membresia.inicio,
                    Corte = request.membresia.corte,
                    TasaBono = request.membresia.tasa_bono,
                    Usuario = request.usuario
                },
                CrearBitacoraLinea(request.usuario, request.cod_tasa_bono, nuevaLinea, MovimientoRegistraWeb, "Tasas Bonificacion"));
        }

        private ErrorDto ActualizarMembresia(int codEmpresa, CrTasasPtsBonificacionMembresiaGuardarRequest request)
        {
            const string sql = @"
                update CRD_TASA_BONO_MEMBRESIA
                   set Modifica_Fecha = dbo.MyGetdate(),
                       Modifica_Usuario = @Usuario,
                       Inicio = @Inicio,
                       Corte = @Corte,
                       Tasa_Bono = @TasaBono
                 where COD_TASA_BONO = @CodTasaBono
                   and Linea = @Linea;";

            return EjecutarGuardarLinea(
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = request.membresia.linea,
                    Inicio = request.membresia.inicio,
                    Corte = request.membresia.corte,
                    TasaBono = request.membresia.tasa_bono,
                    Usuario = request.usuario
                },
                CrearBitacoraLinea(request.usuario, request.cod_tasa_bono, request.membresia.linea, MovimientoModificaWeb, "Tasas Bonificacion"));
        }

        private ErrorDto InsertarDestino(int codEmpresa, CrTasasPtsBonificacionDestinoGuardarRequest request)
        {
            int nuevaLinea = ObtenerSiguienteLinea(codEmpresa, "CRD_TASA_BONO_DESTINO", request.cod_tasa_bono);

            const string sql = @"
                insert into CRD_TASA_BONO_DESTINO
                (
                    COD_TASA_BONO,
                    Linea,
                    COD_DESTINO,
                    PLAZO_INICIO,
                    PLAZO_CORTE,
                    Tasa_Bono,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @CodTasaBono,
                    @Linea,
                    @CodDestino,
                    @PlazoInicio,
                    @PlazoCorte,
                    @TasaBono,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            return EjecutarGuardarLinea(
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = nuevaLinea,
                    CodDestino = request.destino.cod_destino,
                    PlazoInicio = request.destino.plazo_inicio,
                    PlazoCorte = request.destino.plazo_corte,
                    TasaBono = request.destino.tasa_bono,
                    Usuario = request.usuario
                },
                CrearBitacoraLinea(request.usuario, request.cod_tasa_bono, nuevaLinea, MovimientoRegistraWeb, "Tasas Bonificacion, Destinos"));
        }

        private ErrorDto ActualizarDestino(int codEmpresa, CrTasasPtsBonificacionDestinoGuardarRequest request)
        {
            const string sql = @"
                update CRD_TASA_BONO_DESTINO
                   set Modifica_Fecha = dbo.MyGetdate(),
                       Modifica_Usuario = @Usuario,
                       cod_Destino = @CodDestino,
                       PLAZO_INICIO = @PlazoInicio,
                       PLAZO_CORTE = @PlazoCorte,
                       Tasa_Bono = @TasaBono
                 where COD_TASA_BONO = @CodTasaBono
                   and Linea = @Linea;";

            return EjecutarGuardarLinea(
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = request.destino.linea,
                    CodDestino = request.destino.cod_destino,
                    PlazoInicio = request.destino.plazo_inicio,
                    PlazoCorte = request.destino.plazo_corte,
                    TasaBono = request.destino.tasa_bono,
                    Usuario = request.usuario
                },
                CrearBitacoraLinea(request.usuario, request.cod_tasa_bono, request.destino.linea, MovimientoModificaWeb, "Tasas Bonificacion, Destinos"));
        }

        private ErrorDto InsertarLiquidez(int codEmpresa, CrTasasPtsBonificacionLiquidezGuardarRequest request)
        {
            int nuevaLinea = ObtenerSiguienteLinea(codEmpresa, "CRD_TASA_BONO_MEMBRESIA_LIQUIDEZ", request.cod_tasa_bono);

            const string sql = @"
                insert into CRD_TASA_BONO_MEMBRESIA_LIQUIDEZ
                (
                    COD_TASA_BONO,
                    Linea,
                    Cap_Inicial,
                    Cap_Final,
                    Tasa_Bono,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @CodTasaBono,
                    @Linea,
                    @CapInicial,
                    @CapFinal,
                    @TasaBono,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            return EjecutarGuardarLinea(
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = nuevaLinea,
                    CapInicial = request.liquidez.cap_inicial,
                    CapFinal = request.liquidez.cap_final,
                    TasaBono = request.liquidez.tasa_bono,
                    Usuario = request.usuario
                },
                CrearBitacoraLinea(request.usuario, request.cod_tasa_bono, nuevaLinea, MovimientoRegistraWeb, "Tasas Bonificacion, Liquidez"));
        }

        private ErrorDto ActualizarLiquidez(int codEmpresa, CrTasasPtsBonificacionLiquidezGuardarRequest request)
        {
            const string sql = @"
                update CRD_TASA_BONO_MEMBRESIA_LIQUIDEZ
                   set Modifica_Fecha = dbo.MyGetdate(),
                       Modifica_Usuario = @Usuario,
                       Cap_Inicial = @CapInicial,
                       Cap_Final = @CapFinal,
                       Tasa_Bono = @TasaBono
                 where COD_TASA_BONO = @CodTasaBono
                   and Linea = @Linea;";

            return EjecutarGuardarLinea(
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = request.liquidez.linea,
                    CapInicial = request.liquidez.cap_inicial,
                    CapFinal = request.liquidez.cap_final,
                    TasaBono = request.liquidez.tasa_bono,
                    Usuario = request.usuario
                },
                CrearBitacoraLinea(request.usuario, request.cod_tasa_bono, request.liquidez.linea, MovimientoModificaWeb, "Tasas Bonificacion, Liquidez"));
        }

        private ErrorDto InsertarAsignacion(int codEmpresa, CrTasasPtsBonificacionAsignacionGuardarRequest request)
        {
            if (ExisteAsignacion(codEmpresa, request.cod_tasa_bono, request.codigo, request.garantia))
            {
                return new ErrorDto { Code = 0, Description = GuardadoExitoso };
            }

            const string sql = @"
                insert into CRD_TASA_BONO_ASG
                (
                    cod_Tasa_Bono,
                    codigo,
                    garantia,
                    registro_fecha,
                    registro_usuario
                )
                values
                (
                    @CodTasaBono,
                    @Codigo,
                    @Garantia,
                    dbo.MyGetdate(),
                    @Usuario
                );";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Codigo = request.codigo,
                    Garantia = request.garantia,
                    Usuario = request.usuario
                });
        }

        private ErrorDto EjecutarGuardarLinea(
            int codEmpresa,
            string sql,
            object parametros,
            BitacoraLineaInfo bitacora)
        {
            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parametros);
            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(
                codEmpresa,
                bitacora.usuario,
                bitacora.movimiento,
                $"{bitacora.detalle_base}: P:{bitacora.cod_tasa_bono}..L: {bitacora.linea}");

            return new ErrorDto { Code = 0, Description = GuardadoExitoso };
        }

        private static BitacoraLineaInfo CrearBitacoraLinea(
            string usuario,
            string codTasaBono,
            int linea,
            string movimiento,
            string detalleBase)
        {
            return new BitacoraLineaInfo
            {
                usuario = usuario,
                cod_tasa_bono = codTasaBono,
                linea = linea,
                movimiento = movimiento,
                detalle_base = detalleBase
            };
        }

        private ErrorDto EliminarLinea(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request,
            string tabla,
            string detalleBase)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_tasa_bono = Limpiar(request.cod_tasa_bono);

            if (string.IsNullOrWhiteSpace(request.cod_tasa_bono) || request.linea <= 0)
            {
                return Error("Debe indicar el plan y la linea a eliminar.");
            }

            string sql = $@"
                delete from {tabla}
                where cod_Tasa_Bono = @CodTasaBono
                  and Linea = @Linea;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Linea = request.linea
                });

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(codEmpresa, request.usuario, MovimientoEliminaWeb, $"{detalleBase}: P:{request.cod_tasa_bono}..L: {request.linea}");
            return new ErrorDto { Code = 0, Description = EliminadoExitoso };
        }

        private int ObtenerSiguienteLinea(int codEmpresa, string tabla, string codTasaBono)
        {
            string sql = $@"
                select isnull(max(Linea), 0) + 1
                from {tabla}
                where cod_Tasa_Bono = @CodTasaBono;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                1,
                new { CodTasaBono = codTasaBono });

            return resp.Result <= 0 ? 1 : resp.Result;
        }

        private bool ExistePlan(int codEmpresa, string codTasaBono)
        {
            const string sql = @"
                select coalesce(count(*), 0)
                from CRD_TASA_BONO
                where cod_Tasa_Bono = @CodTasaBono;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { CodTasaBono = codTasaBono });

            return resp.Result > 0;
        }

        private bool ExisteAsignacion(int codEmpresa, string codTasaBono, string codigo, string garantia)
        {
            const string sql = @"
                select coalesce(count(*), 0)
                from CRD_TASA_BONO_ASG
                where cod_Tasa_Bono = @CodTasaBono
                  and codigo = @Codigo
                  and garantia = @Garantia;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new
                {
                    CodTasaBono = codTasaBono,
                    Codigo = codigo,
                    Garantia = garantia
                });

            return resp.Result > 0;
        }

        private static string ValidarDefinicion(CrTasasPtsBonificacionDefinicionData definicion)
        {
            if (string.IsNullOrWhiteSpace(definicion.cod_tasa_bono))
            {
                return "Debe indicar el codigo del plan de bonificacion.";
            }

            if (string.IsNullOrWhiteSpace(definicion.descripcion))
            {
                return "Debe indicar la descripcion del plan.";
            }

            return string.IsNullOrWhiteSpace(definicion.notas)
                ? "Debe indicar las notas del plan."
                : string.Empty;
        }

        private static ErrorDto Error(string description)
        {
            return new ErrorDto
            {
                Code = -1,
                Description = description
            };
        }

    }
}
