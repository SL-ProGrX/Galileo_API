using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCargosAplicacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmCrCargosAplicacionDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la lista de cargos adicionales manuales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCargosAplicacionCargoData>> Cr_CargosAplicacion_Cargos_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(COD_CARGO) as item,
                    rtrim(DESCRIPCION) as descripcion,
                    isnull(VALOR, 0) as valor
                from CARGOS_ADICIONALES
                where TIPO = 'M'
                order by COD_CARGO;";

            return DbHelper.ExecuteListQuery<CrCargosAplicacionCargoData>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene la operacion activa y los datos visibles en el formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrCargosAplicacionOperacionData?> Cr_CargosAplicacion_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            if (operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CrCargosAplicacionOperacionData?>(
                    "Debe indicar una operacion valida.",
                    -1,
                    null);
            }

            const string sql = @"
                select top 1
                    Reg.id_solicitud as operacion,
                    rtrim(Soc.cedula) as cedula,
                    rtrim(Soc.nombre) as nombre,
                    rtrim(Cat.codigo) as codigo,
                    rtrim(Cat.descripcion) as linea_desc,
                    rtrim(isnull(Reg.proceso, '')) as proceso,
                    isnull(Reg.opex, 0) as opex
                from socios Soc
                inner join reg_creditos Reg
                    on Soc.cedula = Reg.cedula
                inner join catalogo Cat
                    on Reg.codigo = Cat.codigo
                where Reg.estado = 'A'
                  and Reg.id_solicitud = @Operacion;";

            var response = DbHelper.ExecuteSingleQuery<CrCargosAplicacionOperacionData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion });

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrCargosAplicacionOperacionData?>(
                    response.Description ?? "No fue posible obtener la operacion.",
                    response.Code.GetValueOrDefault(-1),
                    null);
            }

            if (response.Result is null)
            {
                return DbHelper.CreateErrorResponse<CrCargosAplicacionOperacionData?>(
                    "No se encontro operacion activa.",
                    -2,
                    null);
            }

            return DbHelper.CreateOkResponse<CrCargosAplicacionOperacionData?>(response.Result);
        }

        /// <summary>
        /// Aplica un cargo general a la operacion indicada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Cr_CargosAplicacion_Aplicar(
            int codEmpresa,
            CrCargosAplicacionAplicarRequest request)
        {
            request.usuario = NormalizarTexto(request.usuario);
            request.cod_cargo = NormalizarTexto(request.cod_cargo);
            request.notas = (request.notas ?? string.Empty).Trim();

            if (request.operacion <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar una operacion valida."
                };
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar el usuario."
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_cargo))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe seleccionar un cargo."
                };
            }

            if (request.monto <= 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar un monto valido."
                };
            }

            var operacionResp = Cr_CargosAplicacion_Operacion_Obtener(codEmpresa, request.operacion);
            if (operacionResp.Code != 0 || operacionResp.Result is null)
            {
                return new ErrorDto
                {
                    Code = operacionResp.Code.GetValueOrDefault(-1),
                    Description = operacionResp.Description ?? "No se encontro operacion activa."
                };
            }

            var codCuentaResp = Cr_CargosAplicacion_CargoCuenta_Obtener(codEmpresa, request.cod_cargo);
            if (codCuentaResp.Code != 0 || string.IsNullOrWhiteSpace(codCuentaResp.Result))
            {
                return new ErrorDto
                {
                    Code = codCuentaResp.Code.GetValueOrDefault(-1),
                    Description = codCuentaResp.Description ?? "No se encontro el cargo indicado."
                };
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, request.usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return new ErrorDto
                {
                    Code = globalesResp.Code.GetValueOrDefault(-1),
                    Description = globalesResp.Description ?? "No fue posible obtener parametros globales."
                };
            }

            var globales = globalesResp.Result;
            var oficinaUnidad = NormalizarTexto(globales.GOficinaUnidad);
            var oficinaCentroCosto = NormalizarTexto(globales.GOficinaCentroCosto);

            if (string.IsNullOrWhiteSpace(oficinaUnidad) ||
                string.IsNullOrWhiteSpace(oficinaCentroCosto))
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "No fue posible determinar la unidad y centro de costo del usuario."
                };
            }

            const string sql = @"
                exec spCrdOperacionCargoAdd
                    @Operacion,
                    @Monto,
                    @OficinaUnidad,
                    @OficinaCentroCosto,
                    @Notas,
                    @Usuario,
                    'CR',
                    @CodCuenta,
                    @CodCargo,
                    0;";

            var execResp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Operacion = request.operacion,
                    Monto = request.monto,
                    OficinaUnidad = oficinaUnidad,
                    OficinaCentroCosto = oficinaCentroCosto,
                    Notas = TruncarNotas(request.notas),
                    Usuario = request.usuario,
                    CodCuenta = codCuentaResp.Result,
                    CodCargo = request.cod_cargo
                });

            if (execResp.Code != 0)
            {
                return execResp;
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Cargo registrado satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene la cuenta contable del cargo seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCargo"></param>
        /// <returns></returns>
        private ErrorDto<string?> Cr_CargosAplicacion_CargoCuenta_Obtener(
            int codEmpresa,
            string codCargo)
        {
            const string sql = @"
                select top 1
                    rtrim(isnull(COD_CUENTA, ''))
                from CARGOS_ADICIONALES
                where COD_CARGO = @CodCargo;";

            return DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { CodCargo = NormalizarTexto(codCargo) });
        }

        private static string TruncarNotas(string notas)
        {
            var valor = (notas ?? string.Empty).Trim();

            if (valor.Length > 59)
            {
                valor = valor[..59];
            }

            return valor;
        }

        private static string NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}