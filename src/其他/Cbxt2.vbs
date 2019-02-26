'开始
Option Explicit

'获取当前活动的模型 
Dim model ' the current model
Set model = ActiveModel
If (model Is Nothing) Then
    MsgBox "There is no Active Model"
End If

dim parent
set parent= model.findChildByCode("Fk_cb_Adjust_AdjustGUID_cb_AdjustDtl_AdjustGUID", cls_ReferenceJoin)

'更新PD视图
ActiveDiagram.AttachObject(parent)
