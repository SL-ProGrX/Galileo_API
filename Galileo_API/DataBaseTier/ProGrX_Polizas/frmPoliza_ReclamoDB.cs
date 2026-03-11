using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizaReclamoDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizaReclamoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Retorna la lista de motivos de póliza para el formulario frmPoliza_Reclamo.
        /// Ejecuta el SP spPolizas_Motivos según el código de póliza recibido.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codPoliza">Código de póliza.</param>
        /// <returns>Lista de motivos en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Motivos_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(codPoliza))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spPolizas_Motivos @CodPoliza";

                var data = conn.Query<dynamic>(
                    query,
                    new { CodPoliza = codPoliza.Trim() }
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
        /// Retorna la lista de causas de póliza para el formulario frmPoliza_Reclamo.
        /// Ejecuta el SP spPolizas_Causas según el código de póliza recibido.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codPoliza">Código de póliza.</param>
        /// <returns>Lista de causas en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Causas_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(codPoliza))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spPolizas_Causas @CodPoliza";

                var data = conn.Query<dynamic>(
                    query,
                    new { CodPoliza = codPoliza.Trim() }
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
        /// Retorna la lista de estados activos para el seguimiento
        /// del formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de estados activos en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Estados_Lista(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        ID_ESTADO AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM POLIZAS_RECLAMOS_ESTADOS
                    WHERE ACTIVO = 1
                    ORDER BY Descripcion";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Retorna la lista de bancos disponibles para el usuario logueado
        /// en el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario logueado.</param>
        /// <returns>Lista de bancos en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Bancos_Lista(
            int codEmpresa,
            string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(usuario))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spCrd_SGT_Bancos @Usuario";

                var data = conn.Query<dynamic>(
                    query,
                    new { Usuario = usuario.Trim() }
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
        /// Retorna la lista de cuentas bancarias disponibles para una persona
        /// según la cédula y el banco seleccionado en frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="bancoId">Id del banco seleccionado.</param>
        /// <returns>Lista de cuentas bancarias en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Cuentas_Lista(
            int codEmpresa,
            string cedula,
            int bancoId)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(cedula) || bancoId <= 0)
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spSys_Cuentas_Bancarias @Cedula, @BancoId, @Tipo";

                var data = conn.Query<dynamic>(
                    query,
                    new
                    {
                        Cedula = cedula.Trim(),
                        BancoId = bancoId,
                        Tipo = 1
                    }
                ).ToList();

                var result = data.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return result;
            });
        }

    }
}
