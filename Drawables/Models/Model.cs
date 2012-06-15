﻿using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;
using System.Collections.Generic;
using System.Text;

namespace OpenTkProject.Drawables.Models
{
    public class Model : Drawable
    {
        public Model(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }

        public Model(GameObject parent)
        {
            Parent = parent;
            Scene = parent.Scene;
        }

        public virtual void makeUnstatic()
        {
        }

        protected void updateSelection()
        {
            selectedSmooth = selected * 0.05f + selectedSmooth * 0.95f;
        }

        #region drawing

        public override void draw(ViewInfo curView,bool targetLayer)
        {
            if (vaoHandle != null && isVisible && curView.frustrumCheck(this))
            {
                //Vector4 screenpos;
                for (int i = 0; i < vaoHandle.Length; i++)
                {
                    Mesh curMesh = meshes[i];
                    Material curMaterial = materials[i];
                    gameWindow.checkGlError("--uncaught ERROR drawing Model--" + curMesh.name);

                    if (curMaterial.propertys.useAlpha == targetLayer)
                    {
                        //Console.WriteLine("drawing: " + mMesh[i].name);

                        Shader shader = activateMaterial(ref curMaterial);

                        if (shader.loaded)
                        {
                            shader.insertUniform(Shader.Uniform.in_screensize, ref gameWindow.virtual_size);
                            shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);
                            shader.insertUniform(Shader.Uniform.in_time, ref gameWindow.frameTime);
                            shader.insertUniform(Shader.Uniform.in_color, ref color);

                            //GL.Uniform1(curShader.nearLocation, 1, ref mGameWindow.mPlayer.zNear);
                            //GL.Uniform1(curShader.farLocation, 1, ref mGameWindow.mPlayer.zFar);

                            if (Scene != null)
                            {
                                setupMatrices(ref curView, ref shader, ref curMesh);
                            }

                            setSpecialUniforms(ref shader, ref curMesh);

                            GL.BindVertexArray(vaoHandle[i]);
                            GL.DrawElements(BeginMode.Triangles, curMesh.indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);

                            gameWindow.checkGlError("--Drawing ERROR--" + curMesh.name);
                        }
                    }
                }
            }
        }

        protected virtual void setSpecialUniforms(ref Shader curShader, ref Mesh CurMesh)
        {
        }

        public override void drawSelection(ViewInfo curView)
        {
            if (vaoHandle != null && isVisible && curView.frustrumCheck(this))
            {
                for (int i = 0; i < vaoHandle.Length; i++)
                {
                    Shader curShader = activateMaterialSelection(materials[i]);
                    Mesh curMesh = meshes[i];

                    if (curShader.loaded)
                    {
                        if (Scene != null)
                        {
                            setupMatrices(ref curView, ref curShader, ref curMesh);
                        }

                        GL.Uniform1(GL.GetUniformLocation(curShader.handle, "selected"), 1, ref selectedSmooth);

                        GL.BindVertexArray(vaoHandle[i]);
                        GL.DrawElements(BeginMode.Triangles, curMesh.indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    }
                }
            }
        }

        public override void drawNormal(ViewInfo curView)
        {
            if (vaoHandle != null && isVisible && curView.frustrumCheck(this))
            {
                for (int i = 0; i < vaoHandle.Length; i++)
                {
                    Shader curShader = activateMaterialSSN(materials[i]);
                    Mesh curMesh = meshes[i];

                    if (curShader.loaded)
                    {
                        if (Scene != null)
                        {

                            setupMatrices(ref curView, ref curShader, ref curMesh);

                        }

                        GL.BindVertexArray(vaoHandle[i]);
                        GL.DrawElements(BeginMode.Triangles, curMesh.indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    }
                }
            }
        }

        public override void drawShadow(ViewInfo curView)
        {
            if (vaoHandle != null && isVisible && curView.frustrumCheck(this))
            {
                for (int i = 0; i < vaoHandle.Length; i++)
                {
                    Shader shader = activateMaterialShadow(materials[i]);
                    Mesh curMesh = meshes[i];

                    if (shader.loaded)
                    {
                        if (Scene != null)
                        {
                            setupMatrices(ref curView, ref shader, ref curMesh);
                        }

                        shader.insertUniform(Shader.Uniform.in_rendersize, ref gameWindow.currentSize);

                        GL.BindVertexArray(vaoHandle[i]);
                        GL.DrawElements(BeginMode.Triangles, curMesh.indicesVboData.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
                    }
                }
            }
        }

        #endregion drawing/shader

        #region update

        public override void update()
        {
            updateChilds();
        }

        #endregion update

        public override void kill()
        {
            Scene.drawables.Remove(this);

            Parent = null;
            //parent.childNames.Remove(name);

            killChilds();
        }
    }
}
