using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoGruposDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private const int VModulo = 3;

        private const string ReferenciaFormalizacion = "01";
        private const string ReferenciaDesembolso = "02";

        public FrmCrCatalogoGruposDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoGruposDb(PortalDB portalDb, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDb;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de grupos del catálogo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="activos"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoGrupoData>> CrCatalogoGrupos_Obtener(
            int codEmpresa,
            bool? activos)
        {
            string sqlQuery = @"
                select
                    cod_grupo,
                    descripcion,
                    isnull(presu_mensual, 0) as presu_mensual,
                    isnull(presu_diario, 0) as presu_diario,
                    cast(isnull(estado, 0) as bit) as estado
                from catalogo_grupos";

            object? parametros = null;

            if (activos.HasValue)
            {
                sqlQuery += " where estado = @Estado";
                parametros = new
                {
                    Estado = activos.Value ? 1 : 0
                };
            }

            sqlQuery += " order by cod_grupo";

            return DbHelper.ExecuteListQuery<CrCatalogoGrupoData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                parametros
            );
        }

        /// <summary>
        /// Calcula presupuesto, real y diferencia por grupo para el rango solicitado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoGrupoConsultaData>> CrCatalogoGrupos_Consulta_Calcular(
            int codEmpresa,
            CrCatalogoGrupoConsultaRequest request)
        {
            request.referencia = (request.referencia ?? string.Empty).Trim();
            request.grupos = NormalizarGrupos(request.grupos);

            if (request.referencia != ReferenciaFormalizacion &&
                request.referencia != ReferenciaDesembolso)
            {
                return new ErrorDto<List<CrCatalogoGrupoConsultaData>>
                {
                    Code = -1,
                    Description = "La referencia enviada no es válida."
                };
            }

            if (request.fecha_corte.Date < request.fecha_inicio.Date)
            {
                return new ErrorDto<List<CrCatalogoGrupoConsultaData>>
                {
                    Code = -1,
                    Description = "La fecha corte no puede ser menor a la fecha inicio."
                };
            }

            var respBase = CrCatalogoGrupos_Obtener(codEmpresa, request.activos);
            if (respBase.Code < 0)
            {
                return new ErrorDto<List<CrCatalogoGrupoConsultaData>>
                {
                    Code = -1,
                    Description = respBase.Description
                };
            }

            var grupos = respBase.Result ?? new List<CrCatalogoGrupoData>();

            if (request.grupos.Count > 0)
            {
                var gruposSeleccionados = request.grupos.ToHashSet(StringComparer.OrdinalIgnoreCase);
                grupos = grupos
                    .Where(x => gruposSeleccionados.Contains(x.cod_grupo))
                    .ToList();
            }

            var result = new List<CrCatalogoGrupoConsultaData>();

            foreach (var grupo in grupos)
            {
                var respPresupuesto = ObtenerPresupuestoAcumulado(
                    codEmpresa,
                    grupo.cod_grupo,
                    request.fecha_inicio,
                    request.fecha_corte);

                if (respPresupuesto.Code < 0)
                {
                    return new ErrorDto<List<CrCatalogoGrupoConsultaData>>
                    {
                        Code = -1,
                        Description = respPresupuesto.Description
                    };
                }

                var respReal = ObtenerMontoReal(
                    codEmpresa,
                    grupo.cod_grupo,
                    request.referencia,
                    request.fecha_inicio,
                    request.fecha_corte);

                if (respReal.Code < 0)
                {
                    return new ErrorDto<List<CrCatalogoGrupoConsultaData>>
                    {
                        Code = -1,
                        Description = respReal.Description
                    };
                }

                decimal diferencia = respPresupuesto.Result - respReal.Result;

                result.Add(new CrCatalogoGrupoConsultaData
                {
                    cod_grupo = grupo.cod_grupo,
                    descripcion = grupo.descripcion,
                    presupuesto = respPresupuesto.Result,
                    real = respReal.Result,
                    diferencia = diferencia,
                    negativo = diferencia < 0
                });
            }

            return new ErrorDto<List<CrCatalogoGrupoConsultaData>>
            {
                Code = 0,
                Result = result
            };
        }

        /// <summary>
        /// Guarda la definición de un grupo, insertando o actualizando según corresponda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoGrupos_Guardar(
            int codEmpresa,
            string usuario,
            CrCatalogoGrupoData request)
        {
            request.cod_grupo = (request.cod_grupo ?? string.Empty).Trim();
            request.descripcion = (request.descripcion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.cod_grupo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el código del grupo."
                };
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la descripción del grupo."
                };
            }

            var resp = ExisteGrupo(codEmpresa, request.cod_grupo)
                ? ActualizarGrupo(codEmpresa, usuario, request)
                : InsertarGrupo(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene los catálogos asignados y no asignados al grupo seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoGrupoAsignacionCatalogoData>> CrCatalogoGrupos_AsignacionCatalogos_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            const string sqlQuery = @"
                select
                    C.codigo as item,
                    C.descripcion,
                    case
                        when C.retencion = 'S' or C.poliza = 'S' then
                            case when C.convenio = 'S' then 'Ret.Convenio' else 'Retencion' end
                        else
                            case when C.convenio = 'S' then 'Car.Convenio' else 'Cartera' end
                    end as tipo,
                    case
                        when A.codigo is not null then cast(1 as bit)
                        else cast(0 as bit)
                    end as existe
                from catalogo C
                left join catalogo_asignagrp A
                    on C.codigo = A.codigo
                   and A.cod_grupo = @CodGrupo
                order by existe desc, C.codigo";

            return DbHelper.ExecuteListQuery<CrCatalogoGrupoAsignacionCatalogoData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new
                {
                    CodGrupo = (codGrupo ?? string.Empty).Trim()
                }
            );
        }

        /// <summary>
        /// Asigna o desasigna un catálogo al grupo indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoGrupos_Asignacion_Guardar(
            int codEmpresa,
            CrCatalogoGrupoAsignacionGuardarRequest request)
        {
            request.cod_grupo = (request.cod_grupo ?? string.Empty).Trim();
            request.codigo = (request.codigo ?? string.Empty).Trim();
            request.usuario = (request.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.cod_grupo) ||
                string.IsNullOrWhiteSpace(request.codigo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el grupo y el catálogo a procesar."
                };
            }

            string sql = request.isChecked
                ? @"
                if not exists (
                    select 1
                    from catalogo_asignagrp
                    where codigo = @Codigo
                      and cod_grupo = @CodGrupo
                )
                begin
                    insert into catalogo_asignagrp(codigo, cod_grupo)
                    values(@Codigo, @CodGrupo)
                end"
                : @"
                delete from catalogo_asignagrp
                where codigo = @Codigo
                  and cod_grupo = @CodGrupo";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Codigo = request.codigo,
                    CodGrupo = request.cod_grupo
                }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                request.isChecked ? "Registra - WEB" : "Borrar - WEB",
                $"Asignación catálogo: {request.codigo} al grupo: {request.cod_grupo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene el histórico reciente del presupuesto diario de un grupo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoGrupoDiarioData>> CrCatalogoGrupos_Diario_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            const string sqlQuery = @"
                select top 20
                    fecha,
                    presupuesto,
                    usuario,
                    fechai
                from catalogo_grupo_diario
                where cod_grupo = @CodGrupo
                order by fecha desc";

            return DbHelper.ExecuteListQuery<CrCatalogoGrupoDiarioData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                new
                {
                    CodGrupo = (codGrupo ?? string.Empty).Trim()
                }
            );
        }

        /// <summary>
        /// Guarda el presupuesto diario de un grupo, insertando o reemplazando según la solicitud.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoGrupos_Diario_Guardar(
            int codEmpresa,
            CrCatalogoGrupoDiarioGuardarRequest request)
        {
            request.cod_grupo = (request.cod_grupo ?? string.Empty).Trim();
            request.usuario = (request.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.cod_grupo))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar el grupo del presupuesto diario."
                };
            }

            var existe = ExistePresupuestoDiario(codEmpresa, request.cod_grupo, request.fecha);

            if (!existe)
            {
                const string sqlInsert = @"
                    insert into catalogo_grupo_diario(fecha, presupuesto, usuario, fechai, cod_grupo)
                    values(@Fecha, @Presupuesto, @Usuario, dbo.MyGetdate(), @CodGrupo)";

                var respInsert = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sqlInsert,
                    new
                    {
                        Fecha = request.fecha.Date,
                        Presupuesto = request.presupuesto,
                        Usuario = request.usuario,
                        CodGrupo = request.cod_grupo
                    }
                );

                if (respInsert.Code < 0)
                    return respInsert;

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Aplica - WEB",
                    $"Recurso Diario Fecha: {request.fecha:yyyy/MM/dd} Rec:{request.cod_grupo}"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Información guardada satisfactoriamente..."
                };
            }

            if (!request.reemplazar)
            {
                return new ErrorDto
                {
                    Code = 1,
                    Description = "Ya existe un monto presupuestario definido para este día."
                };
            }

            const string sqlUpdate = @"
                update catalogo_grupo_diario
                set presupuesto = @Presupuesto,
                    usuario = @Usuario,
                    fechai = dbo.MyGetdate()
                where cod_grupo = @CodGrupo
                  and cast(fecha as date) = @Fecha";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Fecha = request.fecha.Date,
                    Presupuesto = request.presupuesto,
                    Usuario = request.usuario,
                    CodGrupo = request.cod_grupo
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Modifica - WEB",
                $"Recurso Diario Fecha: {request.fecha:yyyy/MM/dd} Rec:{request.cod_grupo}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Verifica si el grupo ya existe en el catálogo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <returns></returns>
        private bool ExisteGrupo(int codEmpresa, string codGrupo)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from catalogo_grupos
                where cod_grupo = @CodGrupo";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodGrupo = codGrupo.Trim()
                }
            );

            return resp.Result > 0;
        }

        /// <summary>
        /// Verifica si ya existe un presupuesto diario para la fecha y grupo enviados.
        /// </summary>
        private bool ExistePresupuestoDiario(int codEmpresa, string codGrupo, DateTime fecha)
        {
            const string sqlExiste = @"
                select isnull(count(*), 0)
                from catalogo_grupo_diario
                where cod_grupo = @CodGrupo
                  and cast(fecha as date) = @Fecha";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodGrupo = codGrupo.Trim(),
                    Fecha = fecha.Date
                }
            );

            return resp.Result > 0;
        }

        /// <summary>
        /// Inserta un grupo nuevo en el catálogo.
        /// </summary>
        private ErrorDto InsertarGrupo(int codEmpresa, string usuario, CrCatalogoGrupoData request)
        {
            const string sqlInsert = @"
                insert into catalogo_grupos(cod_grupo, descripcion, presu_mensual, presu_diario, estado)
                values(@CodGrupo, @Descripcion, @PresuMensual, @PresuDiario, @Estado)";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodGrupo = request.cod_grupo,
                    Descripcion = request.descripcion,
                    PresuMensual = request.presu_mensual,
                    PresuDiario = request.presu_diario,
                    Estado = request.estado ? 1 : 0
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Registra - WEB",
                $"Grupo Catalogo Adicional Cod: {request.cod_grupo}"
            );

            return respInsert;
        }

        /// <summary>
        /// Actualiza un grupo existente del catálogo.
        /// </summary>
        private ErrorDto ActualizarGrupo(int codEmpresa, string usuario, CrCatalogoGrupoData request)
        {
            const string sqlUpdate = @"
                update catalogo_grupos
                set descripcion = @Descripcion,
                    presu_mensual = @PresuMensual,
                    presu_diario = @PresuDiario,
                    estado = @Estado
                where cod_grupo = @CodGrupo";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodGrupo = request.cod_grupo,
                    Descripcion = request.descripcion,
                    PresuMensual = request.presu_mensual,
                    PresuDiario = request.presu_diario,
                    Estado = request.estado ? 1 : 0
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "Modifica - WEB",
                $"Grupo Catalogo Cod: {request.cod_grupo}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Obtiene el presupuesto acumulado del rango considerando reemplazos diarios.
        /// </summary>
        private ErrorDto<decimal> ObtenerPresupuestoAcumulado(
            int codEmpresa,
            string codGrupo,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            const string sqlQuery = @"
                ;with Fechas as
                (
                    select cast(@FechaInicio as date) as fecha
                    union all
                    select dateadd(day, 1, fecha)
                    from Fechas
                    where fecha < cast(@FechaCorte as date)
                )
                select isnull(sum(isnull(D.presupuesto, G.presu_diario)), 0)
                from Fechas F
                cross join
                (
                    select isnull(presu_diario, 0) as presu_diario
                    from catalogo_grupos
                    where cod_grupo = @CodGrupo
                ) G
                left join catalogo_grupo_diario D
                    on D.cod_grupo = @CodGrupo
                   and cast(D.fecha as date) = F.fecha
                option (maxrecursion 4000)";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                0,
                new
                {
                    CodGrupo = codGrupo.Trim(),
                    FechaInicio = fechaInicio.Date,
                    FechaCorte = fechaCorte.Date
                }
            );
        }

        /// <summary>
        /// Obtiene el monto real desembolsado del grupo en el rango solicitado.
        /// </summary>
        private ErrorDto<decimal> ObtenerMontoReal(
            int codEmpresa,
            string codGrupo,
            string referencia,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            string campoFecha = referencia == ReferenciaFormalizacion
                ? "R.fechaforp"
                : "R.fecha_inicio_calculo";

            string sqlQuery = $@"
                select isnull(sum(R.monto_girado + isnull(DX.desembolso, 0)), 0)
                from reg_creditos R
                outer apply
                (
                    select isnull(sum(D.monto), 0) as desembolso
                    from desembolsos D
                    where D.id_solicitud = R.id_solicitud
                      and D.retener = 0
                ) DX
                where {campoFecha} between @FechaInicio and @FechaCorte
                  and R.estadosol = 'F'
                  and R.monto_girado >= 0
                  and R.cod_grupo = @CodGrupo";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                0,
                new
                {
                    CodGrupo = codGrupo.Trim(),
                    FechaInicio = fechaInicio.Date.AddHours(0),
                    FechaCorte = fechaCorte.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                }
            );
        }

        /// <summary>
        /// Normaliza la lista de grupos seleccionados para cálculos.
        /// </summary>
        private List<string> NormalizarGrupos(List<string>? grupos)
        {
            return (grupos ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Registra movimiento en bitácora.
        /// </summary>
        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}