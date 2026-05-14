using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndVendedoresDb
    {
        private readonly IConfiguration _config;

        private const string SpBancos = "spCrd_SGT_Bancos";

        private const string SqlCuentasBancarias = @"
        SELECT 
            RTRIM(B.Descripcion) AS Banco,
            CASE WHEN C.tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS TipoDesc,
            C.cod_Divisa,
            C.CUENTA_INTERNA AS Cuenta_Interna,
            C.CUENTA_INTERBANCA AS Cuenta_Interbanca,
            C.ACTIVA AS Activa,
            C.DESTINO AS Destino,
            C.REGISTRO_FECHA AS Registro_Fecha,
            C.REGISTRO_USUARIO AS Registro_Usuario
        FROM SYS_CUENTAS_BANCARIAS C
        INNER JOIN TES_BANCOS_GRUPOS B ON C.cod_banco = B.cod_grupo
        WHERE C.Identificacion = @Cedula";

        private const string SqlObtenerVendedor = @"
        SELECT 
            V.cod_vendedor AS Cod_Vendedor,
            V.nombre AS Nombre,
            V.cedula AS Cedula,
            V.estado AS Estado,
            V.aplica_comision AS Aplica_Comision,
            V.cod_banco AS Cod_Banco,
            V.Tipo_Pago,
            V.Minimo,
            V.porc_comision AS Porc_Comision,
            V.Tipo_Id,
            B.Descripcion AS BancoDesc
        FROM fnd_vendedores V
        INNER JOIN tes_Bancos B ON V.Cod_Banco = B.id_Banco
        WHERE V.COD_VENDEDOR = @cod_vendedor";

        private const string SqlListaVendedores = @"
        SELECT 
            cod_Vendedor AS Cod_Vendedor,
            ISNULL(Cedula, '') AS Cedula,
            RTRIM(Nombre) AS Nombre
        FROM fnd_vendedores
        ORDER BY Nombre";

        private const string SqlInsertVendedor = @"
            INSERT INTO fnd_vendedores
                (cod_vendedor, nombre, cedula, estado, aplica_comision, 
                 cod_banco, Tipo_Pago, Minimo, porc_comision, Tipo_Id)
            VALUES
                (@Cod_Vendedor, @Nombre, @Cedula, @Estado, @Aplica_Comision,
                 @Cod_Banco, @Tipo_Pago, @Minimo, @Porc_Comision, @Tipo_Id)";

        private const string SqlUpdateVendedor = @"
            UPDATE fnd_vendedores
            SET nombre = @Nombre,
                cedula = @Cedula,
                estado = @Estado,
                aplica_comision = @Aplica_Comision,
                cod_banco = @Cod_Banco,
                Tipo_Pago = @Tipo_Pago,
                porc_comision = @Porc_Comision,
                Minimo = @Minimo
            WHERE cod_Vendedor = @Cod_Vendedor";

        private const string SqlDeleteVendedor = @"
            DELETE FROM fnd_vendedores
            WHERE cod_vendedor = @cod_vendedor";

        public FrmFndVendedoresDb(IConfiguration config)
        {
            if (config is null)
                throw new ArgumentNullException(nameof(config));
            _config = config;
        }

        /// <summary>
        /// Obtiene cuentas bancarias
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CuentaBancariaVendedorDto>> SYS_CuentasBancarias_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.ExecuteListQuery<CuentaBancariaVendedorDto>(
                new PortalDB(_config),
                codEmpresa,
                SqlCuentasBancarias,
                new { Cedula = NormalizarTexto(cedula) });
        }



        /// <summary>
        /// Obtiene un vendedor por su código
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_vendedor"></param>
        /// <returns></returns>
        public ErrorDto<FndVendedorDto> Fnd_Vendedores_Obtener(int CodEmpresa, int cod_vendedor)
        {
            var result = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlObtenerVendedor,
                new FndVendedorDto(),
                new { cod_vendedor });

            return new ErrorDto<FndVendedorDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndVendedorDto()
            };
        }



        /// <summary>
        /// Obtiene la lista de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            var result = DbHelper.ExecuteStoredProcedureList<DropDownListaGenericaModel>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa),
                SpBancos,
                new { Usuario = NormalizarTexto(Usuario) });

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }



        /// <summary>
        /// Viene a obtener la lista de vendedores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FndVendedorListaDto>> Fnd_Vendedores_Listas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndVendedorListaDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlListaVendedores);
        }


        /// <summary>
        /// Inserta un nuevo vendedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Vendedores_Insertar(int CodEmpresa, FndVendedorDto request)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlInsertVendedor,
                request);
        }



        /// <summary>
        /// Edita un vendedor existente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Vendedores_Actualizar(int CodEmpresa, FndVendedorDto request)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlUpdateVendedor,
                request);
        }


        /// <summary>
        /// Elimina un vendedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_vendedor"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Vendedores_Eliminar(int CodEmpresa, int cod_vendedor)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteVendedor,
                new { cod_vendedor });
        }



        /// <summary>
        /// Scroll para la busqueda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vendedor"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<FndVendedorDto> FND_Vendedor_Scroll_Obtener(int CodEmpresa, int vendedor, int scrollCode)
        {
            var response = new ErrorDto<FndVendedorDto>
            {
                Code = 0,
                Result = new FndVendedorDto()
            };

            try
            {
                var result = DbHelper.ExecuteSingleQuery(
                    new PortalDB(_config),
                    CodEmpresa,
                    ObtenerSqlScroll(scrollCode),
                    vendedor,
                    new { vendedor });

                if (result.Code != 0)
                {
                    response.Code = result.Code;
                    response.Description = result.Description;
                    return response;
                }

                var codVendedor = result.Result == 0 ? vendedor : result.Result;
                return Fnd_Vendedores_Obtener(CodEmpresa, codVendedor);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
                return response;
            }
        }

        private static string ObtenerSqlScroll(int scrollCode)
        {
            return scrollCode == 1
                ? @"SELECT TOP 1 cod_vendedor
                    FROM fnd_vendedores
                    WHERE cod_vendedor > @vendedor
                    ORDER BY cod_vendedor ASC"
                : @"SELECT TOP 1 cod_vendedor
                    FROM fnd_vendedores
                    WHERE cod_vendedor < @vendedor
                    ORDER BY cod_vendedor DESC";
        }

        private static string NormalizarTexto(string? valor)
            => (valor ?? string.Empty).Trim();

    }
}