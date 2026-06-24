using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrPrendasMonitorDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrPrendasMonitorDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de prenda activos para el monitor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_TiposPrenda_Obtener(int codEmpresa)
        {
            const string SqlTiposPrenda = @"
                select
                    rtrim(tipo_prenda) as item,
                    rtrim(descripcion) as descripcion
                from crd_prendas_tipos
                where activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlTiposPrenda
            );
        }

        /// <summary>
        /// Obtiene la lista de catalogo segun tipo para el monitor de prendas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_Catalogo_Obtener(int codEmpresa, string tipo)
        {
            const string SqlCatalogo = @"
                exec spCrd_Prendas_Cat_List_Cbo
                    @Tipo;";

            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<CrPrendasMonitorCatalogoDbItem>(
                    SqlCatalogo,
                    new
                    {
                        Tipo = (tipo ?? string.Empty).Trim()
                    }
                ).ToList()
            );

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result =
                [
                    .. (resp.Result ?? [])
                        .Select(x => new DropDownListaGenericaModel
                        {
                            item = x.IdX,
                            descripcion = x.ItmX
                        })
                ]
            };
        }

        /// <summary>
        /// Obtiene la lista de estados de persona.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_EstadosPersona_Obtener(int codEmpresa)
        {
            const string SqlEstadosPersona = @"
                select
                    rtrim(cod_estado) as item,
                    rtrim(descripcion) as descripcion
                from afi_estados_persona;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlEstadosPersona
            );
        }

        /// <summary>
        /// Obtiene la lista de unidades activas segun el tipo de aplicacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_UnidadesCilindraje_Obtener(int codEmpresa, string tipo)
        {
            string tipoFiltro = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (tipoFiltro != "CIL" && tipoFiltro != "CAP" && tipoFiltro != "PE")
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "El parametro tipo debe ser CIL, CAP o PE.",
                    Result = []
                };
            }

            const string SqlUnidadesCilindraje = @"
                select
                    rtrim(id_unidad) as item,
                    rtrim(descripcion) as descripcion
                from crd_prendas_uds
                where (
                        (@Tipo = 'CIL' and cilindraje_apl = 1)
                     or (@Tipo = 'CAP' and capacidad_apl = 1)
                     or (@Tipo = 'PE' and peso_apl = 1)
                )
                  and activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                SqlUnidadesCilindraje,
                new
                {
                    Tipo = tipoFiltro
                }
            );
        }

        /// <summary>
        /// Obtiene el listado integral de prendas con filtros dinamicos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPrendasMonitorConsultaData>> CrPrendasMonitor_Consulta_Obtener(
            int codEmpresa,
            CrPrendasMonitorConsultaRequest request)
        {
            if (request == null)
            {
                return new ErrorDto<List<CrPrendasMonitorConsultaData>>
                {
                    Code = -1,
                    Description = "Debe enviar el cuerpo de la consulta.",
                    Result = []
                };
            }

            if (request.Fecha_Corte.Date < request.Fecha_Inicio.Date)
            {
                return new ErrorDto<List<CrPrendasMonitorConsultaData>>
                {
                    Code = -1,
                    Description = "La fecha corte no puede ser menor a la fecha inicio.",
                    Result = []
                };
            }

            string campoFecha = ObtenerCampoFecha(request.Tipo_Fecha);
            if (string.IsNullOrWhiteSpace(campoFecha))
            {
                return new ErrorDto<List<CrPrendasMonitorConsultaData>>
                {
                    Code = -1,
                    Description = "El parametro Tipo_Fecha debe ser R, A o P.",
                    Result = []
                };
            }

            var sql = new StringBuilder(@"
                select
                    PRENDA_ID as Prenda_Id,
                    COD_PREANALISIS as Cod_Preanalisis,
                    ID_SOLICITUD as Id_Solicitud,
                    CEDULA as Cedula,
                    NOMBRE as Nombre,
                    TIPO_PRENDA_DESC as Tipo_Prenda_Desc,
                    DESCRIPCION as Descripcion,
                    COBERTURA as Cobertura,
                    PORC_COBERTURA as Porc_Cobertura,
                    ESTADO_DESC as Estado_Desc,
                    ID_PRINCIPAL as Id_Principal,
                    ID_PROVISIONAL as Id_Provisional,
                    AVALUO as Avaluo,
                    VALOR_FISCAL as Valor_Fiscal,
                    VALOR_MERCADO as Valor_Mercado,
                    CREDITO_MONTO as Credito_Monto,
                    CREDITO_SALDO as Credito_Saldo,
                    CREDITO_DIVISA as Credito_Divisa,
                    REGISTRO_FECHA as Registro_Fecha,
                    REGISTRO_USUARIO as Registro_Usuario,
                    ACTUALIZA_FECHA as Actualiza_Fecha,
                    ACTUALIZA_USUARIO as Actualiza_Usuario,
                    COMERCIALIZA_DESC as Comercializa_Desc,
                    MARCA_DESC as Marca_Desc,
                    MODELO_DESC as Modelo_Desc,
                    ANIO as Anio,
                    PRESENTACION_DESC as Presentacion_Desc,
                    SERIE as Serie,
                    COLOR as Color,
                    CHASIS_NUMERO as Chasis_Numero,
                    VIN_MOTOR as Vin_Motor,
                    PUERTAS_NUMERO as Puertas_Numero,
                    PESO as Peso,
                    CAPACIDAD as Capacidad,
                    CILINDRAJE as Cilindraje,
                    TOMO as Tomo,
                    FOLIO as Folio,
                    NOTARIO as Notario,
                    NOTARIO_REGISTRO_FECHA as Notario_Registro_Fecha,
                    POLIZA_MNT_FORMALIZACION as Poliza_Mnt_Formalizacion,
                    POLIZA_RST_PLAN as Poliza_Rst_Plan,
                    PESO_UD_DESC as Peso_Ud_Desc,
                    CAPACIDAD_UD_DESC as Capacidad_Ud_Desc,
                    CILINDRAJE_UD_DESC as Cilindraje_Ud_Desc,
                    case when PE_ACTIVA = 1 then 'Sí' else 'No' end as Pe_Activa,
                    PE_NUMERO as Pe_Numero,
                    PE_VENCE as Pe_Vence,
                    PE_PRIMA as Pe_Prima,
                    PE_FRECUENCIA as Pe_Frecuencia,
                    case when PE_VENCIDA = 1 then 'Sí' else 'No' end as Pe_Vencida,
                    A_CEDULA as Pe_Cedula,
                    A_APELLIDO_1 + ' ' + A_APELLIDO_2 + ' ' + A_NOMBRE as Pe_Nombre,
                    PE_COBERTURA as Pe_Cobertura,
                    case when TITULAR_TERCERO = 1 then 'Sí' else 'No' end as Titular_Tercero,
                    TITULAR_NOMBRE as Titular_Nombre
                from vCrd_Prendas_Integral
                where " + campoFecha + @" >= @FechaInicio
                  and " + campoFecha + @" < @FechaCorte");

            var parametros = new DynamicParameters();
            parametros.Add("FechaInicio", request.Fecha_Inicio.Date);
            parametros.Add("FechaCorte", request.Fecha_Corte.Date.AddDays(1));

            if (request.Pe_Activa.HasValue)
            {
                sql.AppendLine(" and PE_ACTIVA = @PeActiva");
                parametros.Add("PeActiva", request.Pe_Activa.Value ? 1 : 0);
            }

            if (request.Vence_Inicio.HasValue && request.Vence_Corte.HasValue)
            {
                sql.AppendLine(" and PE_VENCE between @VenceInicio and @VenceCorte");
                parametros.Add("VenceInicio", request.Vence_Inicio.Value.Date);
                parametros.Add("VenceCorte", request.Vence_Corte.Value.Date.AddDays(1).AddSeconds(-1));
            }

            AgregarFiltroIgual(sql, parametros, "CREDITO_ESTADO_ID", "CreditoEstadoId", request.Credito_Estado_Id);

            if (request.Id_Presentacion.HasValue)
            {
                sql.AppendLine(" and ID_PRESENTACION = @IdPresentacion");
                parametros.Add("IdPresentacion", request.Id_Presentacion.Value);
            }

            if (request.Id_Combustible.HasValue)
            {
                sql.AppendLine(" and ID_COMBUSTIBLE = @IdCombustible");
                parametros.Add("IdCombustible", request.Id_Combustible.Value);
            }

            if (request.Id_Modelo.HasValue)
            {
                sql.AppendLine(" and ID_MODELO = @IdModelo");
                parametros.Add("IdModelo", request.Id_Modelo.Value);
            }

            AgregarFiltroIgual(sql, parametros, "EstadoActual", "EstadoPersona", request.Estado_Persona);

            if (request.Anio.HasValue)
            {
                sql.AppendLine(" and ANIO = @Anio");
                parametros.Add("Anio", request.Anio.Value);
            }

            if (request.Puertas_Numero.HasValue)
            {
                sql.AppendLine(" and PUERTAS_NUMERO = @PuertasNumero");
                parametros.Add("PuertasNumero", request.Puertas_Numero.Value);
            }

            AgregarFiltroIgual(sql, parametros, "PESO_UD", "UnidadPeso", request.Unidad_Peso);
            AgregarFiltroRango(sql, parametros, "PESO", "PesoInicio", "PesoCorte", request.Peso_Inicio, request.Peso_Corte);

            AgregarFiltroIgual(sql, parametros, "CAPACIDAD_UD", "UnidadCapacidad", request.Unidad_Capacidad);
            AgregarFiltroRango(sql, parametros, "CAPACIDAD", "CapacidadInicio", "CapacidadCorte", request.Capacidad_Inicio, request.Capacidad_Corte);

            AgregarFiltroIgual(sql, parametros, "CILINDRAJE_UD", "UnidadCilindraje", request.Unidad_Cilindraje);
            AgregarFiltroRango(sql, parametros, "CILINDRAJE", "CilindrajeInicio", "CilindrajeCorte", request.Cilindraje_Inicio, request.Cilindraje_Corte);

            AgregarFiltroLista(sql, parametros, "Tipo_Prenda", "TipoPrenda", request.Tipo_Prenda);
            AgregarFiltroLista(sql, parametros, "ID_COMERCIO", "IdComercio", request.Id_Comercio);
            AgregarFiltroLista(sql, parametros, "ID_MARCA", "IdMarca", request.Id_Marca);

            AgregarFiltroLike(sql, parametros, "REGISTRO_USUARIO", "RegistroUsuario", request.Registro_Usuario);
            AgregarFiltroLike(sql, parametros, "ACTUALIZA_USUARIO", "ActualizaUsuario", request.Actualiza_Usuario);
            AgregarFiltroLike(sql, parametros, "CEDULA", "Cedula", request.Cedula);
            AgregarFiltroLike(sql, parametros, "NOMBRE", "Nombre", request.Nombre);
            AgregarFiltroLike(sql, parametros, "ID_PRINCIPAL", "IdPrincipal", request.Id_Principal);
            AgregarFiltroLike(sql, parametros, "ID_PROVISIONAL", "IdProvisional", request.Id_Provisional);
            AgregarFiltroLike(sql, parametros, "CHASIS_NUMERO", "ChasisNumero", request.Chasis_Numero);
            AgregarFiltroLike(sql, parametros, "VIN_MOTOR", "VinMotor", request.Vin_Motor);
            AgregarFiltroLike(sql, parametros, "COLOR", "Color", request.Color);

            sql.AppendLine(" order by REGISTRO_FECHA desc");

            var resp = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<CrPrendasMonitorConsultaData>(
                    sql.ToString(),
                    parametros
                ).ToList()
            );

            return new ErrorDto<List<CrPrendasMonitorConsultaData>>
            {
                Code = resp.Code,
                Description = resp.Description,
                Result = resp.Result ?? []
            };
        }

        private static string ObtenerCampoFecha(string tipoFecha)
        {
            return (tipoFecha ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "R" => "REGISTRO_FECHA",
                "A" => "ACTUALIZA_FECHA",
                "P" => "NOTARIO_REGISTRO_FECHA",
                _ => string.Empty
            };
        }

        private static void AgregarFiltroIgual(
            StringBuilder sql,
            DynamicParameters parametros,
            string columna,
            string nombreParametro,
            string? valor)
        {
            string filtro = (valor ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
                return;

            sql.AppendLine($" and {columna} = @{nombreParametro}");
            parametros.Add(nombreParametro, filtro);
        }

        private static void AgregarFiltroLike(
            StringBuilder sql,
            DynamicParameters parametros,
            string columna,
            string nombreParametro,
            string? valor)
        {
            string filtro = (valor ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
                return;

            sql.AppendLine($" and {columna} like '%' + @{nombreParametro} + '%'");
            parametros.Add(nombreParametro, filtro);
        }

        private static void AgregarFiltroRango(
            StringBuilder sql,
            DynamicParameters parametros,
            string columna,
            string nombreInicio,
            string nombreCorte,
            decimal? inicio,
            decimal? corte)
        {
            if (!inicio.HasValue || !corte.HasValue)
                return;

            sql.AppendLine($" and {columna} between @{nombreInicio} and @{nombreCorte}");
            parametros.Add(nombreInicio, inicio.Value);
            parametros.Add(nombreCorte, corte.Value);
        }

        private static void AgregarFiltroLista(
            StringBuilder sql,
            DynamicParameters parametros,
            string columna,
            string nombreParametro,
            List<string>? valores)
        {
            List<string> lista =
            [
                .. (valores ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            ];

            if (lista.Count == 0)
                return;

            sql.AppendLine($" and {columna} in @{nombreParametro}");
            parametros.Add(nombreParametro, lista);
        }

    }
}
