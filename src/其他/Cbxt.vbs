'开始
Option Explicit

'获取当前活动的模型 
Dim model ' the current model
Set model = ActiveModel
If (model Is Nothing) Then
    MsgBox "There is no Active Model"
End If

'读取Excel
Dim excel
Set excel = CreateObject("Excel.Application")
excel.Workbooks.Open "D:\临时处理\GenDoc\GenDoc\bin\Debug\数据结构-元数据-1.1模块一.xls" '指定 excel文档路径

ImportTableFromExcel excel,model,1
CreateReferenceByExcel excel,model,2
'关闭Excel
excel.Application.Quit
'更新PD视图
ActiveDiagram.AttachAllObjects

'定义方法
Sub ImportTableFromExcel(excel, model,sheetIndex)
  dim rwIndex 
  dim table
  dim col
  
  excel.Workbooks(1).Worksheets(sheetIndex).Activate '指定要打开的sheet名称
  With excel.Workbooks(1).Worksheets(sheetIndex)  
      For rwIndex = 1 To 4687 '指定要遍历的 Excel行标        
              If .Cells(rwIndex, 1).Value = "" Then
                '所在行的第一列为空
                If .Cells(rwIndex,2).Value="" Or .Cells(rwIndex,2).Value="字段" Or .Cells(rwIndex,2).Value="建模标记" Then 
                  '第二列为空
                Else
                  '第二列不为空,则创建列
                  set col = table.Columns.CreateNew '创建一列/字段
                  col.Name = .Cells(rwIndex, 3).Value '指定列名-----如果第1列(Name)为空，则显示第2列的Code                  
                  col.Code = .Cells(rwIndex, 2).Value '指定列名-------第2列是Code
                  col.DataType = .Cells(rwIndex, 5).Value '指定列数据类型-----第3列是类型
                  col.Comment = .Cells(rwIndex, 8).Value '指定列说明-------第5列是列说明
                  If .Cells(rwIndex, 7).Value = "是" Then
                  col.Mandatory = false '指定列是否可空 true 为不可空 ------第4列指定列是否允许为空
                  Else
                      col.Mandatory = true
                  End If
                  If .Cells(rwIndex, 6).Value = "是" Then
                      col.Primary = true '指定主键-------第3行是主键列
                  End If
                End If
              Else 
                '所在行的第一列不为空,则创建表
                set table = model.Tables.CreateNew '创建一个 表实体
                    table.Name = .Cells(rwIndex, 3).Value '指定 表名，如果在 Excel文档里有，也可以 .Cells(rwIndex, 3).Value 这样指定
                    table.Code = .Cells(rwIndex, 3).Value '指定 表名
              End If          
      Next
  End With
Exit Sub
End Sub

Sub CreateReferenceByExcel(excel,model,sheetIndex)
  '获取当前模型中的references
  dim references
  set references = model.references

  '过程中参数定义
  dim parent
  dim pColumn
  dim child
  dim cColumn
  dim newRef
  dim Joins 
  dim rwIndex 

  excel.Workbooks(1).Worksheets(sheetIndex).Activate '指定要打开的sheet名称
  With excel.Workbooks(1).Worksheets(sheetIndex)

    For rwIndex = 2 To 151 '指定要遍历的 Excel行标 由于第1行是列的 表头， 从第2行开始

      if .Cells(rwIndex, 2).Value ="" then 
      else 
      
        set parent= model.findChildByCode(.Cells(rwIndex, 2).Value, cls_Table)
        
        set pColumn= parent.findChildByCode(.Cells(rwIndex, 4).Value, cls_Column)
        
        set child = model.findChildByCode(.Cells(rwIndex, 6).Value, cls_Table)
        
        set cColumn= child.findChildByCode(.Cells(rwIndex, 8).Value, cls_Column)
                 
        set newRef = references.CreateNew()
            newRef.name ="Fk_"+parent.Code+"_"+pColumn.Code+"_"+child.Code+"_"+cColumn.Code
            newRef.code ="Fk_"+parent.Code+"_"+pColumn.Code+"_"+child.Code+"_"+cColumn.Code
        set newref.childTable=child
        set newref.ParentTable=parent
        set Joins=newRef.Joins

        dim j
        for each j in Joins
          set j.ChildTableColumn=cColumn
        next 
      end if

    Next

  End With
  
Exit Sub
End sub
