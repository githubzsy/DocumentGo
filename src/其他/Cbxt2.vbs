'��ʼ
Option Explicit

'��ȡ��ǰ���ģ�� 
Dim model ' the current model
Set model = ActiveModel
If (model Is Nothing) Then
    MsgBox "There is no Active Model"
End If

dim parent
set parent= model.findChildByCode("Fk_cb_Adjust_AdjustGUID_cb_AdjustDtl_AdjustGUID", cls_ReferenceJoin)

'����PD��ͼ
ActiveDiagram.AttachObject(parent)
