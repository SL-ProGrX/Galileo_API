using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCargosRegistroDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;

        public FrmCxCCargosRegistroDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MProGrxMain(config))
        {
        }

        public FrmCxCCargosRegistroDb(
            PortalDB portalDb,
            MProGrxMain mProGrx)
        {
            _portalDb = portalDb;
            _mProGrx = mProGrx;
        }

        /// <summary>
        /// Obtiene la lista de cargos adicionales para el combo principal.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCargosRegistro_CargosAdicionales_Obtener(int codEmpresa)
        {
            const string query = @"
                select
                    rtrim(COD_CARGO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CARGOS_ADICIONALES
                where TIPO = 'M'";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene la operación activa y sus datos visibles en pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxCCargosRegistroOperacionData?> CxCCargosRegistro_Operacion_Obtener(int codEmpresa, int operacion)
        {
            const string query = @"
                select top 1
                    Reg.Operacion as operacion,
                    Soc.cedula as cedula,
                    Soc.nombre as nombre,
                    Cat.cod_concepto as cod_concepto,
                    Cat.descripcion as descripcion,
                    Reg.proceso as proceso,
                    Reg.num_documento as num_documento
                from CxC_Personas Soc
                inner join CxC_Cuentas Reg
                    on Soc.cedula = Reg.cedula
                inner join CxC_Conceptos Cat
                    on Reg.cod_Concepto = Cat.cod_Concepto
                where Reg.estado = 'A'
                  and Reg.Operacion = @operacion;";

            var resp = DbHelper.ExecuteSingleQuery<CxCCargosRegistroOperacionData>(
                    _portalDb,
                    codEmpresa,
                    query,
                    null,
                    new { operacion });
            if (resp.Code == -1 || resp.Result == null)
            {
                return resp;
            }
            var result = resp.Result;

            result.desc_proceso = ObtenerDescripcionProceso(result.proceso);

            return new ErrorDto<CxCCargosRegistroOperacionData?>
            {
                Code = 0,
                Description = string.Empty,
                Result = result
            };
        }

        /// <summary>
        /// Obtiene la configuración del cargo seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCargo"></param>
        /// <returns></returns>
        public ErrorDto<CxCCargosRegistroCargoData?> CxCCargosRegistro_Cargo_Obtener(int codEmpresa, string codCargo)
        {
            const string query = @"
                select top 1
                    rtrim(COD_CARGO) as cod_cargo,
                    rtrim(DESCRIPCION) as descripcion,
                    rtrim(COD_CUENTA) as cod_cuenta
                from CxC_CARGOS
                where COD_CARGO = @codCargo;";

            return DbHelper.ExecuteSingleQuery<CxCCargosRegistroCargoData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { codCargo });
        }

        /// <summary>
        /// Obtiene el monto del cargo por reposición.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxCCargosRegistroCargoReposicionData?> CxCCargosRegistro_CargoReposicion_Obtener(int codEmpresa, int operacion)
        {
            const string query = @"
                select
                    isnull(dbo.fxCxC_CuentaCargoReposicion(@operacion, null), 0) as cargo;";

            return DbHelper.ExecuteSingleQuery<CxCCargosRegistroCargoReposicionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { operacion });
        }

        /// <summary>
        /// Aplica el cargo normal o el cargo por reposición según el request.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CxCCargosRegistro_Aplicar(
            int codEmpresa, string usuario, CxCCargosRegistroAplicarRequest request)
        {
            if (request.operacion <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar una operación válida."
                };
            }

            if (request.monto <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "El monto debe ser mayor a cero."
                };
            }

            if (!request.cargo_reposicion && string.IsNullOrWhiteSpace(request.cod_cargo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe seleccionar un cargo."
                };
            }

            var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code == -1 || globalesResp.Result == null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = globalesResp.Description ?? "No fue posible obtener parámetros globales."
                };
            }

            var globales = globalesResp.Result;
            var oficinaUnidad = NormalizarTexto(globales.GOficinaUnidad);
            var oficinaCentroCosto = NormalizarTexto(globales.GOficinaCentroCosto);

            if (string.IsNullOrWhiteSpace(oficinaUnidad) || string.IsNullOrWhiteSpace(oficinaCentroCosto))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "No fue posible determinar la unidad y centro de costo del usuario."
                };
            }

            ErrorDto resp = request.cargo_reposicion
                ? AplicarCargoReposicion(codEmpresa, usuario, request.operacion, oficinaUnidad, oficinaCentroCosto)
                : AplicarCargoNormal(codEmpresa, usuario, request, oficinaUnidad, oficinaCentroCosto);

            if (resp.Code == -1)
            {
                return resp;
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Cargo registrado satisfactoriamente..."
            };
        }

        private ErrorDto AplicarCargoNormal(
            int codEmpresa,
            string usuario,
            CxCCargosRegistroAplicarRequest request,
            string oficinaUnidad,
            string oficinaCentroCosto)
        {
            const string query = @"
                exec spCxC_CuentaCargoAdd
                    @operacion,
                    @monto,
                    @oficinaUnidad,
                    @oficinaCentroCosto,
                    @notas,
                    @usuario,
                    '',
                    @codCargo,
                    0;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    operacion = request.operacion,
                    monto = request.monto,
                    oficinaUnidad,
                    oficinaCentroCosto,
                    notas = TruncarNotas(request.notas),
                    usuario = NormalizarTexto(usuario),
                    codCargo = NormalizarTexto(request.cod_cargo)
                });
        }

        private ErrorDto AplicarCargoReposicion(
            int codEmpresa,
            string usuario,
            int operacion,
            string oficinaUnidad,
            string oficinaCentroCosto)
        {
            const string query = @"
                exec spCxC_CuentaCargoReposicion
                    @operacion,
                    @usuario,
                    @oficinaUnidad,
                    @oficinaCentroCosto,
                    null;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    operacion,
                    usuario = NormalizarTexto(usuario),
                    oficinaUnidad,
                    oficinaCentroCosto
                });
        }

        private static string ObtenerDescripcionProceso(string proceso)
        {
            return NormalizarTexto(proceso).ToUpper() switch
            {
                "N" => "NORMAL",
                "T" => "TRP.FIADORES",
                "J" => "CBR.JUDICIAL",
                "I" => "INCOBRABLE",
                _ => string.Empty
            };
        }

        private static string TruncarNotas(string? notas)
        {
            var valor = NormalizarTexto(notas);
            return valor.Length <= 59 ? valor : valor[..59];
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }
    }
}
