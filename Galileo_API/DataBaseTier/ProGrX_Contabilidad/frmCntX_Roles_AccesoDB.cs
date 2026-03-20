using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntxRolesAccesoDB
    {

        private readonly PortalDB _portalDb;

        public FrmCntxRolesAccesoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de roles de acceso por contabilidad usando el SP spCntX_AC_Rol_List.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de roles de acceso.</returns>
        public ErrorDto<List<CntXAcRolDto>> CntXAcRol_Lista(int codEmpresa, int codContabilidad, string usuario)
        {
            var sql = "spCntX_AC_Rol_List";
            var parameters = new { Contabilidad = codContabilidad, Usuario = usuario };
            return DbHelper.ExecuteListQuery<CntXAcRolDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta el catálogo de cuentas para un rol de acceso usando el SP spCntX_AC_Cuentas_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="ctaInicio">Cuenta inicial.</param>
        /// <param name="ctaCorte">Cuenta final.</param>
        /// <param name="filtro">Filtro de descripción.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de cuentas disponibles para el rol.</returns>
        public ErrorDto<List<CntXAcCuentaDto>> CntXAcCuentas_Consulta(int codEmpresa, int codContabilidad, string rol, string ctaInicio, string ctaCorte, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Cuentas_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                CtaInicio = ctaInicio,
                CtaCorte = ctaCorte,
                Filtro = filtro,
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcCuentaDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta las cuentas asignadas a un rol de acceso usando el SP spCntX_AC_Cuentas_Consulta_Asignadas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="filtro">Filtro de descripción.</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de cuentas asignadas al rol.</returns>
        public ErrorDto<List<CntXAcCuentaDto>> CntXAcCuentas_Consulta_Asignadas(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Cuentas_Consulta_Asignadas";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Filtro = filtro,
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcCuentaDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta las unidades de negocio disponibles para el rol usando el SP spCntX_AC_Unidades_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="filtro">Filtro de descripción (opcional).</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de unidades de negocio.</returns>
        public ErrorDto<List<CntXAcUnidadDto>> CntXAcUnidades_Consulta(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Unidades_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Filtro = filtro ?? "",
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcUnidadDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta los centros de costo de la unidad de negocio disponibles para el rol usando el SP spCntX_AC_Centro_Costo_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="unidad">Unidad de negocio.</param>
        /// <param name="filtro">Filtro de descripción (opcional).</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de centros de costo.</returns>
        public ErrorDto<List<CntXAcCentroCostoDto>> CntXAcCentroCosto_Consulta(int codEmpresa, int codContabilidad, string rol, string unidad, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Centro_Costo_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Unidad = unidad,
                Filtro = filtro ?? "",
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcCentroCostoDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Consulta los usuarios disponibles y miembros de un rol usando el SP spCntX_AC_Miembros_Consulta.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="rol">Rol de acceso.</param>
        /// <param name="filtro">Filtro de usuario o nombre (opcional).</param>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Lista de usuarios y miembros del rol.</returns>
        public ErrorDto<List<CntXAcMiembroDto>> CntXAcMiembros_Consulta(int codEmpresa, int codContabilidad, string rol, string filtro, string usuario)
        {
            var sql = "spCntX_AC_Miembros_Consulta";
            var parameters = new
            {
                Contabilidad = codContabilidad,
                Rol = rol,
                Filtro = filtro ?? "",
                Usuario = usuario
            };
            return DbHelper.ExecuteListQuery<CntXAcMiembroDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Asigna o elimina una cuenta para un rol de acceso usando el SP spCntX_AC_Cuentas_Asigna.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de asignación/eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXAcCuentas_Asigna(int codEmpresa, CntXAcCuentaAsignaParams param)
        {
            var sql = "spCntX_AC_Cuentas_Asigna";
            var parameters = new
            {
                Contabilidad = param.CodContabilidad,
                param.Rol,
                param.Cuenta,
                param.Usuario,
                param.Mov
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result;
        }

        /// <summary>
        /// Asigna o elimina una unidad para un rol de acceso usando el SP spCntX_AC_Unidades_Asigna.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de asignación/eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXAcUnidades_Asigna(int codEmpresa, CntXAcUnidadAsignaParams param)
        {
            var sql = "spCntX_AC_Unidades_Asigna";
            var parameters = new
            {
                Contabilidad = param.CodContabilidad,
                param.Rol,
                param.Unidad,
                param.Usuario,
                param.Mov
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result;
        }

        /// <summary>
        /// Asigna o elimina un centro de costo para un rol y unidad usando el SP spCntX_AC_Centro_Costo_Asigna.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de asignación/eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXAcCentroCosto_Asigna(int codEmpresa, CntXAcCentroCostoAsignaParams param)
        {
            var sql = "spCntX_AC_Centro_Costo_Asigna";
            var parameters = new
            {
                Contabilidad = param.CodContabilidad,
                param.Rol,
                param.Unidad,
                param.CentroCosto,
                param.Usuario,
                param.Mov
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result;
        }

        /// <summary>
        /// Vincula o desvincula un miembro a un rol usando el SP spCntX_AC_Miembros_Asigna.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de asignación/eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXAcMiembros_Asigna(int codEmpresa, CntXAcMiembroAsignaParams param)
        {
            var sql = "spCntX_AC_Miembros_Asigna";
            var parameters = new
            {
                Contabilidad = param.CodContabilidad,
                param.Rol,
                param.Miembro,
                param.Usuario,
                param.Mov
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result;
        }

        /// <summary>
        /// Agrega un nuevo rol de acceso usando el SP spCntX_AC_Rol_Add.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para agregar el rol.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXAcRol_Add(int codEmpresa, CntXAcRolAddParams param)
        {
            var sql = "spCntX_AC_Rol_Add";
            var parameters = new
            {
                Contabilidad = param.Codigo,
                param.Rol,
                param.Descripcion,
                param.Control,
                param.Activo,
                param.Usuario
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result;
        }

        /// <summary>
        /// Elimina un rol de acceso usando el SP spCntX_AC_Rol_Delete.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para eliminar el rol.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        /// <summary>
        /// Elimina un rol de acceso usando el SP spCntX_AC_Rol_Delete.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para eliminar el rol.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXAcRol_Delete(int codEmpresa, CntXAcRolDeleteParams param)
        {
            var sql = "spCntX_AC_Rol_Delete";
            var parameters = new
            {
                Contabilidad = param.Codigo,
                param.Rol,
                param.Usuario
            };

            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result;
        }
    }
}
