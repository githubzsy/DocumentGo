'��ʼ
Option Explicit

'��ȡ��ǰ���ģ�� 
Dim model ' the current model
Set model = ActiveModel
If (model Is Nothing) Then
    MsgBox "There is no Active Model"
End If

'��ȡExcel
Dim excel
Set excel = CreateObject("Excel.Application")
excel.Workbooks.Open "D:\��ʱ����\GenDoc\GenDoc\bin\Debug\���ݽṹ-Ԫ����-1.1ģ��һ.xls" 'ָ�� excel�ĵ�·��

ImportTableFromExcel excel,model,1
CreateReferenceByExcel excel,model,2
'�ر�Excel
excel.Application.Quit
'����PD��ͼ
ActiveDiagram.AttachAllObjects

'���巽��
Sub ImportTableFromExcel(excel, model,sheetIndex)
  dim rwIndex 
  dim table
  dim col
  
  excel.Workbooks(1).Worksheets(sheetIndex).Activate 'ָ��Ҫ�򿪵�sheet����
  With excel.Workbooks(1).Worksheets(sheetIndex)  
      For rwIndex = 1 To 4687 'ָ��Ҫ������ Excel�б�        
              If .Cells(rwIndex, 1).Value = "" Then
                '�����еĵ�һ��Ϊ��
                If .Cells(rwIndex,2).Value="" Or .Cells(rwIndex,2).Value="�ֶ�" Or .Cells(rwIndex,2).Value="��ģ���" Then 
                  '�ڶ���Ϊ��
                Else
                  '�ڶ��в�Ϊ��,�򴴽���
                  set col = table.Columns.CreateNew '����һ��/�ֶ�
                  col.Name = .Cells(rwIndex, 3).Value 'ָ������-----�����1��(Name)Ϊ�գ�����ʾ��2�е�Code                  
                  col.Code = .Cells(rwIndex, 2).Value 'ָ������-------��2����Code
                  col.DataType = .Cells(rwIndex, 5).Value 'ָ������������-----��3��������
                  col.Comment = .Cells(rwIndex, 8).Value 'ָ����˵��-------��5������˵��
                  If .Cells(rwIndex, 7).Value = "��" Then
                  col.Mandatory = false 'ָ�����Ƿ�ɿ� true Ϊ���ɿ� ------��4��ָ�����Ƿ�����Ϊ��
                  Else
                      col.Mandatory = true
                  End If
                  If .Cells(rwIndex, 6).Value = "��" Then
                      col.Primary = true 'ָ������-------��3����������
                  End If
                End If
              Else 
                '�����еĵ�һ�в�Ϊ��,�򴴽���
                set table = model.Tables.CreateNew '����һ�� ��ʵ��
                    table.Name = .Cells(rwIndex, 3).Value 'ָ�� ����������� Excel�ĵ����У�Ҳ���� .Cells(rwIndex, 3).Value ����ָ��
                    table.Code = .Cells(rwIndex, 3).Value 'ָ�� ����
              End If          
      Next
  End With
Exit Sub
End Sub

Sub CreateReferenceByExcel(excel,model,sheetIndex)
  '��ȡ��ǰģ���е�references
  dim references
  set references = model.references

  '�����в�������
  dim parent
  dim pColumn
  dim child
  dim cColumn
  dim newRef
  dim Joins 
  dim rwIndex 

  excel.Workbooks(1).Worksheets(sheetIndex).Activate 'ָ��Ҫ�򿪵�sheet����
  With excel.Workbooks(1).Worksheets(sheetIndex)

    For rwIndex = 2 To 151 'ָ��Ҫ������ Excel�б� ���ڵ�1�����е� ��ͷ�� �ӵ�2�п�ʼ

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
