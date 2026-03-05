using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizaReclamoInformesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;

        public FrmPolizaReclamoInformesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _proGrxMain.fxFechaServidor(codEmpresa, 0);
        }

        /// <summary>
        /// Retorna la lista de pólizas para el filtro de Informes de Reclamos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de pólizas (Item / Descripcion).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Informes_Polizas_Lista(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT 
                            COD_POLIZA AS item,
                            DESCRIPCION AS descripcion
                        FROM CRD_CATALOGO_POLIZAS
                        ORDER BY DESCRIPCION";

                var result = conn.Query<DropDownListaGenericaModel>(query).ToList();

                return result;
            });
        }


        /// <summary>
        /// Retorna la lista de Estados activos para el filtro
        /// del formulario frmPoliza_Reclamo_Informes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Lista de estados activos (item / descripcion).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
        Poliza_Reclamo_Informes_Estados_Lista(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT 
                            ID_ESTADO AS item,
                            RTRIM(Descripcion) AS descripcion
                        FROM POLIZAS_RECLAMOS_ESTADOS
                        WHERE ACTIVO = 1
                        ORDER BY Descripcion";

                var result = conn.Query<DropDownListaGenericaModel>(query).ToList();

                return result;
            });
        }

        /// <summary>
        /// Retorna la lista de Motivos asociados a una póliza
        /// para el formulario frmPoliza_Reclamo_Informes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codPoliza">Código de póliza.</param>
        /// <returns>Lista de motivos (item / descripcion).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
        Poliza_Reclamo_Informes_Motivos_Lista(int CodEmpresa, string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(codPoliza))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spPolizas_Motivos @CodPoliza";

                var data = conn.Query<dynamic>(
                    query,
                    new { CodPoliza = codPoliza }
                ).ToList();

                var result = data.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return result;
            });
        }

        /// <summary>
        /// Retorna la lista de Causas asociadas a una póliza
        /// para el formulario frmPoliza_Reclamo_Informes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codPoliza">Código de póliza.</param>
        /// <returns>Lista de causas (item / descripcion).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
        Poliza_Reclamo_Informes_Causas_Lista(int CodEmpresa, string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(codPoliza))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spPolizas_Causas @CodPoliza";

                var data = conn.Query<dynamic>(
                    query,
                    new { CodPoliza = codPoliza }
                ).ToList();

                var result = data.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return result;
            });
        }

        /// <summary>
        /// Prepara los filtros del reporte ejecutando spPoliza_Report_Filtro_Add:
        /// 1) Inicializa filtros (Inicializa = 1)
        /// 2) Inserta filtros seleccionados por tipo:
        ///    E = Estados, M = Motivos, C = Causas
        /// Si el ErrorDto devuelve code = 0, el cliente puede proceder a ejecutar el reporte.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario logueado.</param>
        /// <param name="request">Filtros seleccionados.</param>
        public ErrorDto Poliza_Reclamo_Informes_Preparar_Filtros(
            int CodEmpresa, string usuario, PolizaReclamoInformesPrepararFiltrosRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request is null.");

                if (string.IsNullOrWhiteSpace(usuario))
                    return DbHelper.ErrorResponse("usuario es requerido.");

                // 1) Inicializa filtros (equivalente VB6)
                // spPoliza_Report_Filtro_Add(@Tipo varchar(10), @Codigo varchar(30), @Usuario varchar(30), @Inicializa smallint = 0)
                connection.Execute(
                    "EXEC spPoliza_Report_Filtro_Add @Tipo, @Codigo, @Usuario, @Inicializa",
                    new { Tipo = "", Codigo = "", Usuario = "", Inicializa = 1 }
                );

                // 2) Inserta por tipo
                InsertarFiltrosTipo(
                    connection,
                    tipo: "E",
                    usuario: usuario,
                    codPoliza: request.codPoliza,
                    todos: request.todosEstados,
                    codigos: request.estados?.OfType<string>().ToList() ?? new List<string>()
                );
                InsertarFiltrosTipo(
                    connection,
                    tipo: "M",
                    usuario: usuario,
                    codPoliza: request.codPoliza,
                    todos: request.todosMotivos,
                    codigos: request.motivos?.OfType<string>().ToList() ?? new List<string>()
                );
                InsertarFiltrosTipo(
                    connection,
                    tipo: "C",
                    usuario: usuario,
                    codPoliza: request.codPoliza,
                    todos: request.todasCausas,
                    codigos: request.causas?.OfType<string>().ToList() ?? new List<string>()
                );

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Inserta filtros por tipo usando spPoliza_Report_Filtro_Add.
        /// Si "todos" es true y no vienen códigos, se cargan desde DB (para mantener comportamiento VB6).
        /// </summary>
        private void InsertarFiltrosTipo(
            System.Data.IDbConnection connection,
            string tipo,
            string usuario,
            string? codPoliza,
            bool todos,
            List<string> codigos)
        {
            // Si el cliente ya envía los códigos a insertar, los usamos.
            var lista = (codigos ?? new List<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            // Si es "Todos" y no vienen códigos, cargamos todos desde DB (mismo efecto que VB6).
            if (todos && lista.Count == 0)
            {
                lista = CargarTodosCodigosPorTipo(connection, tipo, codPoliza);
            }

            foreach (var codigo in lista)
            {
                connection.Execute(
                    "EXEC spPoliza_Report_Filtro_Add @Tipo, @Codigo, @Usuario, @Inicializa",
                    new { Tipo = tipo, Codigo = codigo, Usuario = usuario, Inicializa = 0 }
                );
            }
        }

        /// <summary>
        /// Carga todos los códigos aplicables según el tipo:
        /// E: estados activos
        /// M: motivos por póliza (requiere codPoliza)
        /// C: causas por póliza (requiere codPoliza)
        /// </summary>
        private List<string> CargarTodosCodigosPorTipo(System.Data.IDbConnection connection, string tipo, string? codPoliza)
        {
            if (tipo == "E")
            {
                const string q = @"SELECT CAST(ID_ESTADO AS VARCHAR(30)) AS Codigo
                                   FROM POLIZAS_RECLAMOS_ESTADOS
                                   WHERE ACTIVO = 1";
                return connection.Query<string>(q).ToList();
            }

            if ((tipo == "M" || tipo == "C") && string.IsNullOrWhiteSpace(codPoliza))
            {
                // Si no hay póliza, no hay forma segura de cargar motivos/causas.
                return new List<string>();
            }

            if (tipo == "M")
            {
                // SP retorna IdX / ItmX, nos interesa IdX
                const string q = @"EXEC spPolizas_Motivos @CodPoliza";
                var data = connection.Query<dynamic>(q, new { CodPoliza = codPoliza }).ToList();
                return data.Select(x => (string)x.IdX).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            }

            if (tipo == "C")
            {
                const string q = @"EXEC spPolizas_Causas @CodPoliza";
                var data = connection.Query<dynamic>(q, new { CodPoliza = codPoliza }).ToList();
                return data.Select(x => (string)x.IdX).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            }

            return new List<string>();
        }

    }
}
