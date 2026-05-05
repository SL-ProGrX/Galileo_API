using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivDetalleGarantiaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCreditos = 1;
        private const string EstadoFormalizada = "F";
        private const string MensajeFormalizada = "No es posible realizar movimientos para un número de operación en estado FORMALIZADA.";

        public FrmVivDetalleGarantiaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }
        /// <summary>
        /// Lista de parámetros de Control de Cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idGarantia"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ErrorDto<VivDetalleGarantiaLista> Viv_DetalleGarantia_Lista_Obtener(int CodEmpresa, int idGarantia, short linea)
        {
            var response = new ErrorDto<VivDetalleGarantiaLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new VivDetalleGarantiaLista()
            };

            try
            {
                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

                const string sql = @"
                    select
                        D.IdGarantia as id_garantia,
                        D.Linea as linea,
                        isnull(rtrim(D.Propietario),'') as propietario,
                        isnull(D.Monto,0) as monto,
                        isnull(rtrim(D.GradoHipoteca),'') as grado_hipoteca,
                        case D.GradoHipoteca
                            when 'P' then 'Primer Grado'
                            when 'S' then 'Segundo Grado'
                            when 'T' then 'Tercer Grado'
                            else ''
                        end as desc_grado_hipoteca,
                        isnull(rtrim(D.Observaciones),'') as observaciones,
                        isnull(rtrim(D.RegistroUsuario),'') as registro_usuario,
                        cast(0 as bit) as isNew
                    from ViviendaGarantiaDetalle D
                    where D.IdGarantia = @idGarantia
                      and (@linea = -1 or D.Linea = @linea)
                    order by D.Linea;";

                var lista = conn.Query<VivDetalleGarantiaData>(sql, new { idGarantia, linea }).ToList();

                response.Result.total = lista.Count;
                response.Result.lista = lista;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<VivDetalleGarantiaLista>(ex.Message, -1, response.Result);
            }
        }
        /// <summary>
        /// Obtiene grado hipotecario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="descGradoHipoteca"></param>
        /// <returns></returns>
        public ErrorDto<List<VivDetalleGarantiaGradoItem>> Viv_DetalleGarantia_Grados_Dropdown_Obtener(int CodEmpresa, string descGradoHipoteca)
        {
            var grado = (descGradoHipoteca ?? string.Empty).Trim();
            var lista = new List<VivDetalleGarantiaGradoItem>();

            if (grado.Equals("Segundo Grado", StringComparison.OrdinalIgnoreCase))
            {
                lista.Add(new VivDetalleGarantiaGradoItem { item = "P", descripcion = "Primer Grado" });
            }
            else if (grado.Equals("Tercer Grado", StringComparison.OrdinalIgnoreCase))
            {
                lista.Add(new VivDetalleGarantiaGradoItem { item = "P", descripcion = "Primer Grado" });
                lista.Add(new VivDetalleGarantiaGradoItem { item = "S", descripcion = "Segundo Grado" });
            }

            return new ErrorDto<List<VivDetalleGarantiaGradoItem>>
            {
                Code = 0,
                Description = "Ok",
                Result = lista
            };
        }
        /// <summary>
        /// Guardar detalle garantia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Viv_DetalleGarantia_Guardar(int CodEmpresa, VivDetalleGarantiaGuardarDto data, string usuario)
        {
            try
            {
                var validacion = ValidarGuardar(CodEmpresa, data);
                if (validacion.Code != 0) return validacion;

                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

                var linea = data.isNew ? (short)-1 : data.linea;

                var parametros = new DynamicParameters();
                parametros.Add("@IdGarantia", data.id_garantia);
                parametros.Add("@Linea", linea);
                parametros.Add("@Propietario", data.propietario.Trim());
                parametros.Add("@Monto", data.monto);
                parametros.Add("@GradoHipoteca", NormalizarGradoHipoteca(data.grado_hipoteca));
                parametros.Add("@Observaciones", string.IsNullOrWhiteSpace(data.observaciones) ? null : data.observaciones.Trim());
                parametros.Add("@RegistroUsuario", usuario.Trim());

                conn.Execute("spCrdVivGarantiaDetalle_A", parametros, commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.Trim(),
                    DetalleMovimiento = $"Garantias vivienda hipoteca: {data.id_garantia} monto: {data.monto}",
                    Movimiento = data.isNew ? "REGISTRA-WEB" : "MODIFICA-WEB",
                    Modulo = ModuloCreditos
                });

                return new ErrorDto { Code = 0, Description = "Operación realizada correctamente." };
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Elimina detalle garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Viv_DetalleGarantia_Eliminar(int CodEmpresa, VivDetalleGarantiaEliminarDto data)
        {
            try
            {
                if (OperacionFormalizada(CodEmpresa, data.id_garantia))
                    return DbHelper.ErrorResponse(MensajeFormalizada, -2);

                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

                const string sql = @"
            delete ViviendaGarantiaDetalle
            where IdGarantia = @idGarantia
              and Linea = @linea;";

                var rows = conn.Execute(sql, new
                {
                    idGarantia = data.id_garantia,
                    linea = data.linea
                });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se encontró la línea seleccionada para borrar.", -2);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (data.usuario ?? string.Empty).Trim(),
                    DetalleMovimiento = $"Garantias vivienda hipoteca: {data.id_garantia} linea: {data.linea}",
                    Movimiento = "ELIMINA-WEB",
                    Modulo = ModuloCreditos
                });

                return new ErrorDto
                {
                    Code = 0,
                    Description = "La información seleccionada fue borrada correctamente"
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private ErrorDto ValidarGuardar(int CodEmpresa, VivDetalleGarantiaGuardarDto data)
        {
            if (OperacionFormalizada(CodEmpresa, data.id_garantia))
                return DbHelper.ErrorResponse(MensajeFormalizada, -2);

            if (string.IsNullOrWhiteSpace(data.propietario))
                return DbHelper.ErrorResponse("Debe de ingresar un nombre para el propietario.", -2);

            if (data.monto <= 0)
                return DbHelper.ErrorResponse("Debe de ingresar un monto válido.", -2);

            return new ErrorDto { Code = 0 };
        }
        private bool OperacionFormalizada(int CodEmpresa, int idGarantia)
        {
            return EstadoOperacion(CodEmpresa, idGarantia) == EstadoFormalizada;
        }
        private string EstadoOperacion(int CodEmpresa, int idGarantia)
        {
            using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

            return conn.QueryFirstOrDefault<string>(@"
                select top 1 R.ESTADOSOL
                from ViviendaGarantia G
                inner join REG_CREDITOS R on G.NumeroOperacion = R.ID_SOLICITUD
                where G.IdGarantia = @idGarantia",
                new { idGarantia }) ?? "";
        }
        private static string NormalizarGradoHipoteca(string grado)
        {
            grado = (grado ?? "").Trim().ToUpper();
            return grado switch
            {
                "PRIMER GRADO" => "P",
                "SEGUNDO GRADO" => "S",
                "TERCER GRADO" => "T",
                _ => grado
            };
        }
    }
}