namespace Galileo.DataBaseTier
{
    public partial class MTesoreria
    {
        private static class Sql
        {
            public const string TesTiposDocumentosObtener =
                @"select TIPO AS ITEM, DESCRIPCION from tes_tipos_doc";

            public const string TesUnidadesCargaUsuario = @"
select rtrim(C.cod_unidad) as item, rtrim(C.descripcion) as descripcion
from tes_unidad_ASG A
inner join CntX_Unidades C on A.cod_unidad = C.cod_unidad and C.cod_contabilidad = @contabilidad
where A.id_Banco = @banco and A.nombre = @usuario and activa = 1
order by C.Descripcion";

            public const string TesConceptosCargaUsuario = @"
select rtrim(C.cod_Concepto) as item, rtrim(C.Descripcion) as descripcion
from tes_conceptos_ASG A
inner join Tes_Conceptos C on A.cod_concepto = C.cod_concepto
where A.id_Banco = @banco and A.nombre = @usuario and estado = 'A'
order by C.Descripcion";


            private const string TesTipoAccesoSolicita = @"
select isnull(Count(*),0) as Existe
from tes_documentos_ASG A
inner join Tes_Tipos_Doc T on A.tipo = T.tipo
where A.id_Banco = @banco and A.nombre = @usuario and A.tipo = @tipo
  and isnull(A.SOLICITA,0) = 1";

            private const string TesTipoAccesoAutoriza = @"
select isnull(Count(*),0) as Existe
from tes_documentos_ASG A
inner join Tes_Tipos_Doc T on A.tipo = T.tipo
where A.id_Banco = @banco and A.nombre = @usuario and A.tipo = @tipo
  and isnull(A.AUTORIZA,0) = 1";

            private const string TesTipoAccesoGenera = @"
select isnull(Count(*),0) as Existe
from tes_documentos_ASG A
inner join Tes_Tipos_Doc T on A.tipo = T.tipo
where A.id_Banco = @banco and A.nombre = @usuario and A.tipo = @tipo
  and isnull(A.GENERA,0) = 1";

            private const string TesTipoAccesoAsientos = @"
select isnull(Count(*),0) as Existe
from tes_documentos_ASG A
inner join Tes_Tipos_Doc T on A.tipo = T.tipo
where A.id_Banco = @banco and A.nombre = @usuario and A.tipo = @tipo
  and isnull(A.ASIENTOS,0) = 1";

            private const string TesTipoAccesoAnula = @"
select isnull(Count(*),0) as Existe
from tes_documentos_ASG A
inner join Tes_Tipos_Doc T on A.tipo = T.tipo
where A.id_Banco = @banco and A.nombre = @usuario and A.tipo = @tipo
  and isnull(A.ANULA,0) = 1";

            public static string GetTesTipoAccesoValidaByPermiso(string permiso) => permiso switch
            {
                Mappers.PERM_SOLICITA => TesTipoAccesoSolicita,
                Mappers.PERM_AUTORIZA => TesTipoAccesoAutoriza,
                Mappers.PERM_GENERA => TesTipoAccesoGenera,
                Mappers.PERM_ASIENTOS => TesTipoAccesoAsientos,
                Mappers.PERM_ANULA => TesTipoAccesoAnula,
                _ => TesTipoAccesoSolicita
            };

            // ---- Permisos: sbTesBancoCargaCboAccesoGestion ----
            private const string TesBancoAccesoSolicita = @"
select id_banco as item, descripcion
from Tes_Bancos
where Estado = 'A'
  and id_Banco in (
      select id_banco
      from tes_documentos_ASG
      where nombre = @usuario and isnull(SOLICITA,0) = 1
      group by id_banco
  )";

            private const string TesBancoAccesoAutoriza = @"
select id_banco as item, descripcion
from Tes_Bancos
where Estado = 'A'
  and id_Banco in (
      select id_banco
      from tes_documentos_ASG
      where nombre = @usuario and isnull(AUTORIZA,0) = 1
      group by id_banco
  )";

            private const string TesBancoAccesoGenera = @"
select id_banco as item, descripcion
from Tes_Bancos
where Estado = 'A'
  and id_Banco in (
      select id_banco
      from tes_documentos_ASG
      where nombre = @usuario and isnull(GENERA,0) = 1
      group by id_banco
  )";

            private const string TesBancoAccesoAsientos = @"
select id_banco as item, descripcion
from Tes_Bancos
where Estado = 'A'
  and id_Banco in (
      select id_banco
      from tes_documentos_ASG
      where nombre = @usuario and isnull(ASIENTOS,0) = 1
      group by id_banco
  )";

            private const string TesBancoAccesoAnula = @"
select id_banco as item, descripcion
from Tes_Bancos
where Estado = 'A'
  and id_Banco in (
      select id_banco
      from tes_documentos_ASG
      where nombre = @usuario and isnull(ANULA,0) = 1
      group by id_banco
  )";

            public static string GetTesBancoCargaCboAccesoGestionByPermiso(string permiso) => permiso switch
            {
                Mappers.PERM_SOLICITA => TesBancoAccesoSolicita,
                Mappers.PERM_AUTORIZA => TesBancoAccesoAutoriza,
                Mappers.PERM_GENERA => TesBancoAccesoGenera,
                Mappers.PERM_ASIENTOS => TesBancoAccesoAsientos,
                Mappers.PERM_ANULA => TesBancoAccesoAnula,
                _ => TesBancoAccesoSolicita
            };

            // ---- Campo tes_banco_docs: sin columnas dinámicas ----
            public const string TesBancoDocsCampoPorTipoBanco = @"
select
  case @campo
    when 'Comprobante' then Comprobante
    when 'Consecutivo' then cast(Consecutivo as varchar(50))
    when 'CONSECUTIVO_DET' then cast(CONSECUTIVO_DET as varchar(50))
    when 'DOC_AUTO' then cast(DOC_AUTO as varchar(50))
    when 'Movimiento' then Movimiento
    else null
  end as Campo
from tes_Banco_docs
where id_Banco = @banco and tipo = @tipo";

            // ---- Permisos: sbTesTiposDocsCargaCboAcceso ----
            private const string TesTiposDocsAccesoSolicita = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND isnull(A.SOLICITA,0) = 1
ORDER BY T.Descripcion";

            private const string TesTiposDocsAccesoAutoriza = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND isnull(A.AUTORIZA,0) = 1
ORDER BY T.Descripcion";

            private const string TesTiposDocsAccesoGenera = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND isnull(A.GENERA,0) = 1
ORDER BY T.Descripcion";

            private const string TesTiposDocsAccesoAsientos = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND isnull(A.ASIENTOS,0) = 1
ORDER BY T.Descripcion";

            private const string TesTiposDocsAccesoAnula = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND isnull(A.ANULA,0) = 1
ORDER BY T.Descripcion";

            public static string GetTesTiposDocsCargaCboAccesoByPermiso(string permiso) => permiso switch
            {
                Mappers.PERM_SOLICITA => TesTiposDocsAccesoSolicita,
                Mappers.PERM_AUTORIZA => TesTiposDocsAccesoAutoriza,
                Mappers.PERM_GENERA => TesTiposDocsAccesoGenera,
                Mappers.PERM_ASIENTOS => TesTiposDocsAccesoAsientos,
                Mappers.PERM_ANULA => TesTiposDocsAccesoAnula,
                _ => TesTiposDocsAccesoSolicita
            };

            // ---- Permisos: sbTesTiposDocsCargaCboAccesoFirmas ----
            private const string TesTiposDocsAccesoFirmasSolicita = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND D.comprobante = '01'
  AND isnull(A.SOLICITA,0) = 1";

            private const string TesTiposDocsAccesoFirmasAutoriza = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND D.comprobante = '01'
  AND isnull(A.AUTORIZA,0) = 1";

            private const string TesTiposDocsAccesoFirmasGenera = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND D.comprobante = '01'
  AND isnull(A.GENERA,0) = 1";

            private const string TesTiposDocsAccesoFirmasAsientos = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND D.comprobante = '01'
  AND isnull(A.ASIENTOS,0) = 1";

            private const string TesTiposDocsAccesoFirmasAnula = @"
SELECT RTRIM(T.Tipo) + ' - ' + T.descripcion AS ItmY,
       RTRIM(T.Tipo) AS item, RTRIM(T.descripcion) AS descripcion
FROM tes_documentos_ASG A
INNER JOIN Tes_Tipos_Doc T ON A.tipo = T.tipo
INNER JOIN TES_BANCO_DOCS D ON T.tipo = D.tipo AND A.id_banco = D.id_Banco
WHERE A.id_Banco = @banco AND A.nombre = @usuario
  AND D.comprobante = '01'
  AND isnull(A.ANULA,0) = 1";

            public static string GetTesTiposDocsCargaCboAccesoFirmasByPermiso(string permiso) => permiso switch
            {
                Mappers.PERM_SOLICITA => TesTiposDocsAccesoFirmasSolicita,
                Mappers.PERM_AUTORIZA => TesTiposDocsAccesoFirmasAutoriza,
                Mappers.PERM_GENERA => TesTiposDocsAccesoFirmasGenera,
                Mappers.PERM_ASIENTOS => TesTiposDocsAccesoFirmasAsientos,
                Mappers.PERM_ANULA => TesTiposDocsAccesoFirmasAnula,
                _ => TesTiposDocsAccesoFirmasSolicita
            };

            // ---- Consecutivos ----
            public const string TesBancoDocsConsecutivo =
                @"SELECT ISNULL(Consecutivo,0) FROM tes_banco_docs WHERE tipo = @Tipo AND id_banco = @Banco";

            public const string TesBancoPlanesTeConsecutivo =
                @"SELECT ISNULL(NUMERO_TE,0) FROM TES_BANCO_PLANES_TE WHERE id_banco = @Banco AND COD_PLAN = @Plan";

            public const string UpdateTesBancoDocsConsecutivoByAvance = @"
UPDATE tes_banco_docs
SET consecutivo = ISNULL(consecutivo,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
WHERE Tipo = @Tipo AND id_banco = @Banco";

            public const string UpdateTesBancoPlanesTeNumeroTeByAvance = @"
UPDATE TES_BANCO_PLANES_TE
SET NUMERO_TE = ISNULL(NUMERO_TE,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
WHERE COD_PLAN = @Plan AND id_banco = @Banco";

            public const string TesBancoDocsConsecutivoDet =
                @"SELECT ISNULL(CONSECUTIVO_DET,0) FROM tes_banco_docs WHERE tipo = @Tipo AND id_banco = @Banco";

            public const string TesBancoPlanesTeNumeroInterno =
                @"SELECT ISNULL(NUMERO_INTERNO,0) FROM TES_BANCO_PLANES_TE WHERE id_banco = @Banco AND COD_PLAN = @Plan";

            public const string UpdateTesBancoDocsConsecutivoDetByAvance = @"
UPDATE tes_banco_docs
SET CONSECUTIVO_DET = ISNULL(CONSECUTIVO_DET,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
WHERE Tipo = @Tipo AND id_banco = @Banco";

            public const string UpdateTesBancoPlanesTeNumeroInternoByAvance = @"
UPDATE TES_BANCO_PLANES_TE
SET NUMERO_INTERNO = ISNULL(NUMERO_INTERNO,0) + CASE @avance WHEN '+' THEN 1 WHEN '-' THEN -1 ELSE 0 END
WHERE COD_PLAN = @Plan AND id_banco = @Banco";

            // ---- Archivos especiales ----
            public const string TesBancosArchivosEspeciales =
                @"Select UTILIZA_FORMATO_ESPECIAL,ARCHIVO_CHEQUES_FIRMAS,ARCHIVO_CHEQUES_SIN_FIRMAS from TES_BANCOS where ID_BANCO = @banco";

            // ---- SPs / utilitarios ----
            public const string TesAfectaBancos = "exec spTESAfectaBancos @solicitud, @tipo";
            public const string TesBitacora = "exec spTesBitacora @solicitud, @movimiento, @detalle, @usuario";

            // ---- Cargas generales ----
            public const string TesBancosActivos = @"
select id_banco as item, descripcion
from Tes_Bancos
where Estado = 'A'
order by descripcion";

            public const string TesUnidadesCargaGeneral = @"
select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion
from CntX_Unidades
where cod_contabilidad = @contabilidad";

            public const string TesTiposDocsPorBanco = @"
select T.Tipo as item, rtrim(T.Descripcion) as descripcion
from tes_banco_docs A
inner join Tes_Tipos_Doc T on A.tipo = T.tipo
where A.id_Banco = @banco
order by T.Descripcion";

            public const string TesConceptosGeneral =
                @"select rtrim(cod_Concepto) as item, rtrim(Descripcion) as descripcion from Tes_Conceptos";

            // ---- Actualiza CC ----
            public const string UpdateDesembolsosBancoDoc = @"
UPDATE DesemBolsos
SET Cod_Banco = @Banco,
    TDocumento = @Tipo,
    NDocumento = @Documento
WHERE ID_Desembolso = @Codigo";

            public const string UpdateRegCreditosBancoDoc = @"
UPDATE Reg_Creditos
SET Cod_Banco = @Banco,
    Documento_Referido = @documentoReferido
WHERE ID_Solicitud = @OP";

            // ---- Parámetros / validaciones ----
            public const string TesParametroPorCodigo = "select valor from tes_parametros where cod_parametro = @codigo";
            public const string EmpresaSinpeActivo = "select sinpe_activo from SIF_EMPRESA WHERE PORTAL_ID = @empresa";

            public const string TesBancoValida = @"
select isnull(Count(*),0) as Existe
from Tes_Bancos B
inner join tes_Banco_ASG A on B.id_Banco = A.id_Banco
where A.nombre = @usuario
  and B.estado = 'A'
  and B.id_Banco = @banco";

            public const string TesConceptoValida = @"
select isnull(Count(*),0) as Existe
from tes_conceptos_ASG A
inner join Tes_Conceptos C on A.cod_concepto = C.cod_concepto
where A.id_Banco = @banco
  and A.nombre = @usuario
  and A.cod_concepto = @concepto";

            public const string TesUnidadValida = @"
select isnull(Count(*),0) as Existe
from tes_unidad_ASG A
inner join CntX_Unidades C on A.cod_unidad = C.cod_unidad
where A.id_Banco = @banco
  and A.nombre = @usuario
  and A.cod_unidad = @unidad";

            public const string TesDocumentoExisteNoPendiente = @"
select isnull(count(*),0) as Existe
from Tes_Transacciones
where id_banco = @banco
  and tipo = @tipo
  and Ndocumento = @documento
  and estado <> 'P'";

            public const string TesTiposDocMovimiento =
                @"select Movimiento from tes_tipos_doc Where tipo = @tipo";

            public const string TesCuentaDestinoObligatoria = @"
select isnull(count(*),0) as Existe
from Tes_Bancos
where id_banco = @banco
  and INT_REQUIERE_CUENTA_DESTINO = 1";

            // ---- Tokens ----
            public const string TesTokenConsulta = @"exec spTes_Token_Consulta '', 'A' , @usuario";

            public const string TesTokenUltimoPorUsuario = @"
select top 1 ID_TOKEN
from Tes_Tokens
where REGISTRO_USUARIO = @usuario
order by REGISTRO_FECHA desc";

            // ---- Permisos: fxValidaPermisoUserBancosTipo ----

            private const string ValidaPermisoSolicita = @"
select count(T.Tipo)
from tes_tipos_doc T
left join tes_documentos_asg A on T.tipo = A.tipo
    and A.id_banco = @banco and A.nombre = @usuario
where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)
  AND T.Tipo = @tipo
  AND isnull(A.SOLICITA,0) = 1";

            private const string ValidaPermisoAutoriza = @"
select count(T.Tipo)
from tes_tipos_doc T
left join tes_documentos_asg A on T.tipo = A.tipo
    and A.id_banco = @banco and A.nombre = @usuario
where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)
  AND T.Tipo = @tipo
  AND isnull(A.AUTORIZA,0) = 1";

            private const string ValidaPermisoGenera = @"
select count(T.Tipo)
from tes_tipos_doc T
left join tes_documentos_asg A on T.tipo = A.tipo
    and A.id_banco = @banco and A.nombre = @usuario
where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)
  AND T.Tipo = @tipo
  AND isnull(A.GENERA,0) = 1";

            private const string ValidaPermisoAsientos = @"
select count(T.Tipo)
from tes_tipos_doc T
left join tes_documentos_asg A on T.tipo = A.tipo
    and A.id_banco = @banco and A.nombre = @usuario
where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)
  AND T.Tipo = @tipo
  AND isnull(A.ASIENTOS,0) = 1";

            private const string ValidaPermisoAnula = @"
select count(T.Tipo)
from tes_tipos_doc T
left join tes_documentos_asg A on T.tipo = A.tipo
    and A.id_banco = @banco and A.nombre = @usuario
where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)
  AND T.Tipo = @tipo
  AND isnull(A.ANULA,0) = 1";

            public static string GetValidaPermisoUserBancosTipoByPermiso(string permiso) => permiso switch
            {
                Mappers.PERM_SOLICITA => ValidaPermisoSolicita,
                Mappers.PERM_AUTORIZA => ValidaPermisoAutoriza,
                Mappers.PERM_GENERA => ValidaPermisoGenera,
                Mappers.PERM_ASIENTOS => ValidaPermisoAsientos,
                Mappers.PERM_ANULA => ValidaPermisoAnula,
                _ => ValidaPermisoSolicita
            };
        }
    }
}