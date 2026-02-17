using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasAjustesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb DBBitacora;
        private readonly int vModulo = 31;

        public FrmCxCCuentasAjustesDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCxCCuentasAjustesDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            DBBitacora = dbBitacora;
        }

        public ErrorDto<CxCCuentasAjustesOperacionData> CxCCuentasAjustes_ConsultaOperacion_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select R.*,S.nombre,C.descripcion, 
                Ofi.Descripcion as 'OficinaX',Cnt.Descripcion as 'Contrato',Pag.Nombre as 'Pagador' 
                from CxC_Cuentas R inner join CxC_Conceptos C on R.cod_concepto = C.cod_concepto 
                inner join CxC_Personas S on R.cedula = S.cedula  
                left join CxC_Contratos Cnt on R.Cod_Contrato = Cnt.Cod_Contrato 
                left Join CxC_Personas Pag on R.cedula_pagador = Pag.cedula 
                left join SIF_Oficinas Ofi on R.cod_oficina = Ofi.cod_Oficina 
                where R.estado = 'A' and R.proceso <> 'J' and R.Operacion = @operacionId";

            var result = DbHelper.ExecuteSingleQuery<CxCCuentasAjustesOperacionData>(
                _portalDb, codEmpresa, query, new CxCCuentasAjustesOperacionData(), new { operacionId });

            if (result.Result == null)
            {
                result.Result = new CxCCuentasAjustesOperacionData();
            }
            return result!;
        }

        public ErrorDto<List<CxCCuentasAjustesCuotasData>> CxCCuentasAjustes_CuotasMora_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select * From CxC_Cuentas_Mov 
                where Dias_Mora > 0 AND ESTADO = 'A' AND Operacion = @operacionId";

            return DbHelper.ExecuteListQuery<CxCCuentasAjustesCuotasData>(_portalDb, codEmpresa, query, new { operacionId });
        }

        public ErrorDto<List<CxCCuentasAjustesCargosData>> CxCCuentasAjustes_Cargos_Obtener(int codEmpresa, int operacionId)
        {
            const string query = @"select * from CxC_Cuentas_Cargos 
                where Operacion = @operacionId and Monto = Saldo;";

            return DbHelper.ExecuteListQuery<CxCCuentasAjustesCargosData>(_portalDb, codEmpresa, query, new { operacionId });
        }

        public ErrorDto CxCCuentasAjustes_Fecha_Aplicar(int codEmpresa, CxCCuentasAjustesFechaRequest request)
        {
            const string query = @"exec spCxC_CuentaIntereses @operacionId, '', @fechaDoc";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { operacionId = request.operacion, fechaDoc = request.fecha_documento });
        }

        public ErrorDto CxCCuentasAjustes_CuotasMora_Eliminar(int codEmpresa, int operacionId, string usuario, List<CxCCuentasAjustesCuotasData> lista)
        {
            const string query = @"
                UPDATE CxC_Cuentas_Mov 
                SET Dias_Mora = 0, 
                    Int_Mor   = 0
                WHERE Linea = @linea 
                  AND Operacion = @operacionId;";

            foreach (var item in lista)
            {
                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    query,
                    new { linea = item.linea, operacionId });

                if (resp.Code == -1)
                    return resp;

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? "").ToUpper(),
                    DetalleMovimiento = $"Morosidad OP: {operacionId} Linea: {item.linea}",
                    Movimiento = "Anula - WEB",
                    Modulo = vModulo
                });
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Reversiones realizadas satisfactoriamente..."
            };
        }

        public ErrorDto CxCCuentasAjustes_Cargos_Eliminar(int codEmpresa, int operacionId, string usuario, List<CxCCuentasAjustesCargosData> lista)
        {
            const string query = @"exec spCxC_CuentaCargoElimina @operacionId, @linea;";

            foreach (var item in lista)
            {
                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    query,
                    new { linea = item.id_cargo, operacionId });

                if (resp.Code == -1)
                    return resp;

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? "").ToUpper(),
                    DetalleMovimiento = $"Cargos OP: {operacionId} Id: {item.id_cargo} Monto..: {item.monto}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "Reversiones realizada satisfactoriamente..."
            };
        }
    }
}
