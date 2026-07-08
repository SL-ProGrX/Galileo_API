using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrTasasPtsBonificacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        private const int VModulo = 3;
        private const string GuardadoExitoso = "Informacion guardada satisfactoriamente...";
        private const string EliminadoExitoso = "Informacion eliminada satisfactoriamente...";

        public FrmCrTasasPtsBonificacionDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrTasasPtsBonificacionDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene los planes de tasas por puntos de bonificacion registrados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionPlanData>> CrTasasPtsBonificacion_Planes_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(isnull(cod_Tasa_Bono, '')) as cod_tasa_bono,
                    rtrim(isnull(Descripcion, '')) as descripcion,
                    rtrim(isnull(Notas, '')) as notas,
                    cast(isnull(Activo, 0) as bit) as activo,
                    rtrim(isnull(Registro_Usuario, '')) as registro_usuario,
                    Registro_Fecha as registro_fecha
                from CRD_TASA_BONO
                order by cod_Tasa_Bono;";

            return DbHelper.ExecuteListQuery<CrTasasPtsBonificacionPlanData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el plan anterior o siguiente segun el desplazamiento solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="codTasaBono"></param>
        /// <returns></returns>
        public ErrorDto<CrTasasPtsBonificacionDefinicionData?> CrTasasPtsBonificacion_Scroll_Obtener(
            int codEmpresa,
            int scroll,
            string codTasaBono)
        {
            codTasaBono = Limpiar(codTasaBono);

            string sqlNext = scroll == 1
                ? @"select top 1 cod_Tasa_Bono
                    from CRD_TASA_BONO
                    where cod_Tasa_Bono > @CodTasaBono
                    order by cod_Tasa_Bono asc;"
                : @"select top 1 cod_Tasa_Bono
                    from CRD_TASA_BONO
                    where cod_Tasa_Bono < @CodTasaBono
                    order by cod_Tasa_Bono desc;";

            var nextId = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sqlNext,
                null,
                new { CodTasaBono = codTasaBono });

            if (!string.IsNullOrWhiteSpace(nextId.Result))
            {
                return CrTasasPtsBonificacion_Definicion_Obtener(codEmpresa, nextId.Result);
            }

            return new ErrorDto<CrTasasPtsBonificacionDefinicionData?>
            {
                Code = -2,
                Description = "No se encontraron mas resultados.",
                Result = null
            };
        }

        /// <summary>
        /// Obtiene la definicion de un plan de tasas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codTasaBono"></param>
        /// <returns></returns>
        public ErrorDto<CrTasasPtsBonificacionDefinicionData?> CrTasasPtsBonificacion_Definicion_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            codTasaBono = Limpiar(codTasaBono);

            const string sql = @"
                select
                    rtrim(isnull(cod_Tasa_Bono, '')) as cod_tasa_bono,
                    rtrim(isnull(Descripcion, '')) as descripcion,
                    rtrim(isnull(Notas, '')) as notas,
                    cast(isnull(Activo, 0) as bit) as activo
                from CRD_TASA_BONO
                where cod_Tasa_Bono = @CodTasaBono;";

            return DbHelper.ExecuteSingleQuery<CrTasasPtsBonificacionDefinicionData>(
                _portalDb,
                codEmpresa,
                sql,
                new CrTasasPtsBonificacionDefinicionData(),
                new { CodTasaBono = codTasaBono });
        }

        /// <summary>
        /// Guarda la definicion del plan de tasas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Definicion_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionDefinicionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.codigo_original = Limpiar(request.codigo_original);
            request.definicion.cod_tasa_bono = Limpiar(request.definicion.cod_tasa_bono);
            request.definicion.descripcion = (request.definicion.descripcion ?? string.Empty).Trim();
            request.definicion.notas = (request.definicion.notas ?? string.Empty).Trim();

            string mensaje = ValidarDefinicion(request.definicion);
            if (!string.IsNullOrWhiteSpace(mensaje))
            {
                return Error(mensaje);
            }

            if (!request.editar)
            {
                if (ExistePlan(codEmpresa, request.definicion.cod_tasa_bono))
                {
                    return Error("El codigo del plan ya existe.");
                }

                return InsertarPlan(codEmpresa, request);
            }

            if (!string.Equals(
                request.codigo_original,
                request.definicion.cod_tasa_bono,
                StringComparison.OrdinalIgnoreCase))
            {
                return Error("Ha modificado el codigo del plan.");
            }

            return ActualizarPlan(codEmpresa, request);
        }

        /// <summary>
        /// Elimina el plan de tasas y sus detalles asociados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Definicion_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionDefinicionEliminarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_tasa_bono = Limpiar(request.cod_tasa_bono);

            if (string.IsNullOrWhiteSpace(request.cod_tasa_bono))
            {
                return Error("Debe indicar el codigo del plan de bonificacion.");
            }

            string[] deletes =
            {
                "delete from CRD_TASA_BONO_ASG where cod_Tasa_Bono = @CodTasaBono;",
                "delete from CRD_TASA_BONO_DESTINO where cod_Tasa_Bono = @CodTasaBono;",
                "delete from CRD_TASA_BONO_MEMBRESIA_LIQUIDEZ where cod_Tasa_Bono = @CodTasaBono;",
                "delete from CRD_TASA_BONO_MEMBRESIA where cod_Tasa_Bono = @CodTasaBono;",
                "delete from CRD_TASA_BONO where cod_Tasa_Bono = @CodTasaBono;"
            };

            foreach (string sql in deletes)
            {
                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new { CodTasaBono = request.cod_tasa_bono });

                if (resp.Code < 0)
                {
                    return resp;
                }
            }

            RegistrarBitacora(codEmpresa, request.usuario, "Elimina - WEB", $"Tasa: Plan de Bonificacion : {request.cod_tasa_bono}");

            return new ErrorDto { Code = 0, Description = EliminadoExitoso };
        }

        /// <summary>
        /// Obtiene los puntos de bonificacion por membresia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codTasaBono"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionMembresiaData>> CrTasasPtsBonificacion_Membresias_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            const string sql = @"
                select
                    isnull(Linea, 0) as linea,
                    isnull(Inicio, 0) as inicio,
                    isnull(Corte, 0) as corte,
                    isnull(Tasa_Bono, 0) as tasa_bono,
                    rtrim(isnull(Registro_Usuario, '')) as registro_usuario,
                    Registro_Fecha as registro_fecha,
                    rtrim(isnull(Modifica_Usuario, '')) as modifica_usuario,
                    Modifica_Fecha as modifica_fecha
                from CRD_TASA_BONO_MEMBRESIA
                where cod_Tasa_Bono = @CodTasaBono
                order by Linea;";

            return DbHelper.ExecuteListQuery<CrTasasPtsBonificacionMembresiaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodTasaBono = Limpiar(codTasaBono) });
        }

        /// <summary>
        /// Guarda una linea de puntos por membresia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Membresias_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionMembresiaGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_tasa_bono = Limpiar(request.cod_tasa_bono);

            if (string.IsNullOrWhiteSpace(request.cod_tasa_bono))
            {
                return Error("Debe indicar el codigo del plan de bonificacion.");
            }

            if (request.membresia.inicio <= 0 || request.membresia.corte <= 0)
            {
                return Error("Debe indicar valores validos para inicio y corte.");
            }

            if (request.membresia.linea <= 0)
            {
                return InsertarMembresia(codEmpresa, request);
            }

            return ActualizarMembresia(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una linea de puntos por membresia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Membresias_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
            => EliminarLinea(codEmpresa, request, "CRD_TASA_BONO_MEMBRESIA", "Tasas Bonificacion");

        /// <summary>
        /// Obtiene los puntos de bonificacion por destino.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codTasaBono"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionDestinoData>> CrTasasPtsBonificacion_Destinos_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            const string sql = @"
                select
                    isnull(T.Linea, 0) as linea,
                    rtrim(isnull(T.Cod_Destino, '')) as cod_destino,
                    rtrim(isnull(D.Descripcion, '')) as destino_desc,
                    isnull(T.Plazo_Inicio, 0) as plazo_inicio,
                    isnull(T.Plazo_Corte, 0) as plazo_corte,
                    isnull(T.Tasa_Bono, 0) as tasa_bono,
                    rtrim(isnull(T.Registro_Usuario, '')) as registro_usuario,
                    T.Registro_Fecha as registro_fecha,
                    rtrim(isnull(T.Modifica_Usuario, '')) as modifica_usuario,
                    T.Modifica_Fecha as modifica_fecha
                from CRD_TASA_BONO_DESTINO T
                left join CATALOGO_DESTINOS D on T.Cod_Destino = D.Cod_Destino
                where T.cod_Tasa_Bono = @CodTasaBono
                order by T.Cod_Destino, T.Plazo_Inicio;";

            return DbHelper.ExecuteListQuery<CrTasasPtsBonificacionDestinoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodTasaBono = Limpiar(codTasaBono) });
        }

        /// <summary>
        /// Guarda una linea de puntos por destino.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Destinos_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionDestinoGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_tasa_bono = Limpiar(request.cod_tasa_bono);
            request.destino.cod_destino = Limpiar(request.destino.cod_destino);

            if (string.IsNullOrWhiteSpace(request.cod_tasa_bono) || string.IsNullOrWhiteSpace(request.destino.cod_destino))
            {
                return Error("Debe indicar el plan y el destino.");
            }

            if (request.destino.plazo_inicio <= 0 || request.destino.plazo_corte <= 0)
            {
                return Error("Debe indicar valores validos para plazo inicio y plazo corte.");
            }

            if (request.destino.linea <= 0)
            {
                return InsertarDestino(codEmpresa, request);
            }

            return ActualizarDestino(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una linea de puntos por destino.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Destinos_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
            => EliminarLinea(codEmpresa, request, "CRD_TASA_BONO_DESTINO", "Tasas Bonificacion, Destinos");

        /// <summary>
        /// Obtiene los puntos de bonificacion por liquidez.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codTasaBono"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionLiquidezData>> CrTasasPtsBonificacion_Liquidez_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            const string sql = @"
                select
                    isnull(Linea, 0) as linea,
                    isnull(Cap_Inicial, 0) as cap_inicial,
                    isnull(Cap_Final, 0) as cap_final,
                    isnull(Tasa_Bono, 0) as tasa_bono,
                    rtrim(isnull(Registro_Usuario, '')) as registro_usuario,
                    Registro_Fecha as registro_fecha,
                    rtrim(isnull(Modifica_Usuario, '')) as modifica_usuario,
                    Modifica_Fecha as modifica_fecha
                from CRD_TASA_BONO_MEMBRESIA_LIQUIDEZ
                where cod_Tasa_Bono = @CodTasaBono
                order by Linea;";

            return DbHelper.ExecuteListQuery<CrTasasPtsBonificacionLiquidezData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodTasaBono = Limpiar(codTasaBono) });
        }

        /// <summary>
        /// Guarda una linea de puntos por liquidez.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Liquidez_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionLiquidezGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_tasa_bono = Limpiar(request.cod_tasa_bono);

            if (string.IsNullOrWhiteSpace(request.cod_tasa_bono))
            {
                return Error("Debe indicar el codigo del plan de bonificacion.");
            }

            if (request.liquidez.cap_inicial <= 0 || request.liquidez.cap_final <= 0)
            {
                return Error("Debe indicar valores validos para capital inicial y capital final.");
            }

            if (request.liquidez.linea <= 0)
            {
                return InsertarLiquidez(codEmpresa, request);
            }

            return ActualizarLiquidez(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una linea de puntos por liquidez.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Liquidez_Eliminar(
            int codEmpresa,
            CrTasasPtsBonificacionLineaEliminarRequest request)
            => EliminarLinea(codEmpresa, request, "CRD_TASA_BONO_MEMBRESIA_LIQUIDEZ", "Tasas Bonificacion, Liquidez");

        /// <summary>
        /// Obtiene las lineas y garantias asignables del plan.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codTasaBono"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionAsignacionLineaData>> CrTasasPtsBonificacion_Asignaciones_Obtener(
            int codEmpresa,
            string codTasaBono)
        {
            const string sqlLineas = @"
                select
                    rtrim(isnull(codigo, '')) as codigo,
                    rtrim(isnull(descripcion, '')) as descripcion
                from catalogo
                where retencion = 'N'
                  and Poliza = 'N'
                  and Activo = 1
                order by codigo;";

            var lineas = DbHelper.ExecuteListQuery<CrTasasPtsBonificacionAsignacionLineaData>(
                _portalDb,
                codEmpresa,
                sqlLineas);

            if (lineas.Code < 0)
            {
                return lineas;
            }

            const string sqlGarantias = @"
                select
                    rtrim(isnull(C.codigo, '')) as codigo,
                    rtrim(isnull(T.garantia, '')) as garantia,
                    rtrim(isnull(T.descripcion, '')) as descripcion,
                    cast(case when A.registro_fecha is null then 0 else 1 end as bit) as asignado
                from crd_catalogo_garantias C
                inner join crd_garantia_tipos T on C.garantia = T.garantia
                left join CRD_TASA_BONO_ASG A
                    on A.codigo = C.codigo
                   and A.garantia = T.garantia
                   and A.cod_Tasa_Bono = @CodTasaBono
                order by C.codigo, T.descripcion;";

            var garantias = DbHelper.ExecuteListQuery<CrTasasPtsBonificacionAsignacionGarantiaData>(
                _portalDb,
                codEmpresa,
                sqlGarantias,
                new { CodTasaBono = Limpiar(codTasaBono) });

            if (garantias.Code < 0)
            {
                return new ErrorDto<List<CrTasasPtsBonificacionAsignacionLineaData>>
                {
                    Code = garantias.Code,
                    Description = garantias.Description,
                    Result = lineas.Result
                };
            }

            foreach (var linea in lineas.Result ?? new List<CrTasasPtsBonificacionAsignacionLineaData>())
            {
                linea.garantias = (garantias.Result ?? new List<CrTasasPtsBonificacionAsignacionGarantiaData>())
                    .Where(x => string.Equals(x.codigo, linea.codigo, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return lineas;
        }

        /// <summary>
        /// Obtiene los planes asignables para una linea y garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="garantia"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionAsignacionPlanData>> CrTasasPtsBonificacion_AsignacionPlanes_Obtener(
            int codEmpresa,
            string codigo,
            string garantia)
        {
            codigo = Limpiar(codigo);
            garantia = Limpiar(garantia);

            const string sql = @"
                select
                    rtrim(isnull(R.cod_Tasa_Bono, '')) as cod_tasa_bono,
                    rtrim(isnull(R.Descripcion, '')) as descripcion,
                    cast(case when A.codigo is null then 0 else 1 end as bit) as asignado
                from CRD_TASA_BONO R
                left join CRD_TASA_BONO_ASG A
                    on R.cod_Tasa_Bono = A.cod_Tasa_Bono
                   and A.codigo = @Codigo
                   and A.Garantia = @Garantia
                order by case when A.codigo is null then 1 else 0 end,
                         R.cod_Tasa_Bono;";

            return DbHelper.ExecuteListQuery<CrTasasPtsBonificacionAsignacionPlanData>(
                _portalDb,
                codEmpresa,
                sql,
                new { Codigo = codigo, Garantia = garantia });
        }

        /// <summary>
        /// Guarda o elimina una asignacion por linea y garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrTasasPtsBonificacion_Asignaciones_Guardar(
            int codEmpresa,
            CrTasasPtsBonificacionAsignacionGuardarRequest request)
        {
            request.usuario = Limpiar(request.usuario);
            request.cod_tasa_bono = Limpiar(request.cod_tasa_bono);
            request.codigo = Limpiar(request.codigo);
            request.garantia = Limpiar(request.garantia);

            if (string.IsNullOrWhiteSpace(request.cod_tasa_bono)
                || string.IsNullOrWhiteSpace(request.codigo)
                || string.IsNullOrWhiteSpace(request.garantia))
            {
                return Error("Debe indicar el plan, la linea y la garantia.");
            }

            if (request.asignado)
            {
                return InsertarAsignacion(codEmpresa, request);
            }

            const string sqlDelete = @"
                delete from CRD_TASA_BONO_ASG
                where cod_Tasa_Bono = @CodTasaBono
                  and codigo = @Codigo
                  and garantia = @Garantia;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodTasaBono = request.cod_tasa_bono,
                    Codigo = request.codigo,
                    Garantia = request.garantia
                });
        }

        /// <summary>
        /// Obtiene el catalogo de destinos para busqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrTasasPtsBonificacionDestinoCatalogoData>> CrTasasPtsBonificacion_DestinosCatalogo_Obtener(
            int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(isnull(Cod_Destino, '')) as cod_destino,
                    rtrim(isnull(Descripcion, '')) as descripcion
                from Catalogo_Destinos
                order by Cod_Destino;";

            return DbHelper.ExecuteListQuery<CrTasasPtsBonificacionDestinoCatalogoData>(_portalDb, codEmpresa, sql);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }

        private static string Limpiar(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}

