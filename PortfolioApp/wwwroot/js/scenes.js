/* Three.js scenes for the portfolio.
   1) #sceneHero  - full hero background: floating wireframe shapes + particle field,
                    sits BEHIND the headline, buttons and portrait.
   2) #sceneWave  - interactive particle wave band above the footer.
   Palette matches the site: ink #1a1a18, accent #c65d34, warm off-white background. */

(function () {
  if (typeof THREE === 'undefined') return;

  const INK = 0x1a1a18;
  const ACCENT = 0xc65d34;
  const MUTED = 0x9a958a;
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const isMobile = window.matchMedia('(max-width: 800px)').matches;

  function makeRenderer(container) {
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, isMobile ? 1.5 : 2));
    renderer.domElement.style.cssText = 'position:absolute;inset:0;width:100%;height:100%;display:block';
    container.appendChild(renderer.domElement);
    return renderer;
  }

  function fit(renderer, camera, container) {
    const w = container.clientWidth, h = container.clientHeight;
    if (!w || !h) return;
    renderer.setSize(w, h, false);
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
  }

  // Only animate while on screen (saves battery / CPU)
  function onVisible(el, cb) {
    const io = new IntersectionObserver(entries => entries.forEach(e => cb(e.isIntersecting)), { threshold: 0.02 });
    io.observe(el);
  }

  /* ---------- Scene 1: Hero background ---------- */
    function initHero() {
        const container = document.getElementById('sceneHero');
        if (!container) return;

        // =========================================================
        // HERO 3D SCENE
        // =========================================================

        const scene = new THREE.Scene();

        const camera = new THREE.PerspectiveCamera(
            55,
            1,
            0.1,
            100
        );

        camera.position.set(0, 0, 15);

        const renderer = makeRenderer(container);

        // =========================================================
        // MATERIAL HELPERS
        // =========================================================

        function wireMaterial(color, opacity = 0.45) {
            return new THREE.MeshBasicMaterial({
                color: color,
                wireframe: true,
                transparent: true,
                opacity: opacity
            });
        }

        // =========================================================
        // FLOATING GEOMETRY
        // =========================================================

        const objects = [];

        const geometries = [

            // Main developer-style geometry
            new THREE.IcosahedronGeometry(1, 1),

            new THREE.OctahedronGeometry(1, 0),

            new THREE.DodecahedronGeometry(1, 0),

            new THREE.TetrahedronGeometry(1.1, 0),

            new THREE.TorusGeometry(
                1,
                0.20,
                16,
                48
            ),

            new THREE.TorusKnotGeometry(
                0.75,
                0.18,
                64,
                10
            ),

            new THREE.BoxGeometry(
                1.3,
                1.3,
                1.3
            )
        ];

        // =========================================================
        // CREATE FLOATING OBJECT
        // =========================================================

        function createShape(
            geometry,
            x,
            y,
            z,
            scale,
            color,
            opacity
        ) {

            const mesh = new THREE.Mesh(
                geometry,
                wireMaterial(color, opacity)
            );

            mesh.position.set(x, y, z);

            mesh.scale.setScalar(scale);

            mesh.rotation.x = Math.random() * Math.PI;
            mesh.rotation.y = Math.random() * Math.PI;
            mesh.rotation.z = Math.random() * Math.PI;

            mesh.userData = {

                baseX: x,
                baseY: y,
                baseZ: z,

                floatSpeed:
                    0.15 + Math.random() * 0.25,

                floatAmplitude:
                    0.15 + Math.random() * 0.35,

                rotationX:
                    (Math.random() - 0.5) * 0.25,

                rotationY:
                    (Math.random() - 0.5) * 0.30,

                rotationZ:
                    (Math.random() - 0.5) * 0.15,

                phase:
                    Math.random() * Math.PI * 2
            };

            scene.add(mesh);
            objects.push(mesh);
        }

        // =========================================================
        // DESKTOP LAYOUT
        // =========================================================

        if (!isMobile) {

            // -----------------------------------------------------
            // LEFT SIDE
            // -----------------------------------------------------

            createShape(
                geometries[0],
                -7.5,
                3.5,
                -5,
                1.0,
                ACCENT,
                0.45
            );

            createShape(
                geometries[1],
                -5.8,
                -2.8,
                -4,
                0.65,
                INK,
                0.25
            );

            createShape(
                geometries[4],
                -7.0,
                -0.5,
                -6,
                1.0,
                ACCENT,
                0.30
            );

            // -----------------------------------------------------
            // CENTER
            // -----------------------------------------------------

            createShape(
                geometries[2],
                -1.0,
                3.8,
                -7,
                0.65,
                INK,
                0.18
            );

            createShape(
                geometries[3],
                1.8,
                -3.5,
                -6,
                0.75,
                ACCENT,
                0.28
            );

            createShape(
                geometries[5],
                3.0,
                3.5,
                -7,
                0.65,
                INK,
                0.20
            );

            // -----------------------------------------------------
            // RIGHT / PORTRAIT AREA
            // -----------------------------------------------------

            createShape(
                geometries[0],
                7.5,
                3.5,
                -4,
                1.15,
                ACCENT,
                0.55
            );

            createShape(
                geometries[4],
                8.5,
                -2.5,
                -5,
                1.1,
                INK,
                0.35
            );

            createShape(
                geometries[5],
                6.5,
                0.2,
                -6,
                0.75,
                ACCENT,
                0.35
            );

            createShape(
                geometries[6],
                9.0,
                1.8,
                -8,
                0.65,
                INK,
                0.18
            );

        } else {

            // =====================================================
            // MOBILE LAYOUT
            // =====================================================

            createShape(
                geometries[0],
                -3.2,
                4.0,
                -5,
                0.7,
                ACCENT,
                0.35
            );

            createShape(
                geometries[4],
                3.0,
                3.5,
                -5,
                0.7,
                INK,
                0.25
            );

            createShape(
                geometries[2],
                -3.0,
                -2.0,
                -6,
                0.55,
                INK,
                0.20
            );

            createShape(
                geometries[5],
                3.0,
                -2.5,
                -6,
                0.55,
                ACCENT,
                0.25
            );

            createShape(
                geometries[1],
                0,
                4.8,
                -8,
                0.5,
                ACCENT,
                0.25
            );
        }

        // =========================================================
        // LARGE BACKGROUND RINGS
        // =========================================================

        const ringGeometry =
            new THREE.TorusGeometry(
                4.5,
                0.025,
                12,
                100
            );

        const ringMaterial =
            new THREE.MeshBasicMaterial({
                color: ACCENT,
                transparent: true,
                opacity: 0.12
            });

        const ring1 =
            new THREE.Mesh(
                ringGeometry,
                ringMaterial
            );

        ring1.position.set(
            5,
            0,
            -10
        );

        ring1.rotation.x =
            Math.PI * 0.35;

        ring1.rotation.y =
            Math.PI * 0.20;

        scene.add(ring1);

        const ring2 =
            new THREE.Mesh(
                ringGeometry,
                new THREE.MeshBasicMaterial({
                    color: INK,
                    transparent: true,
                    opacity: 0.08
                })
            );

        ring2.position.set(
            -5,
            1,
            -11
        );

        ring2.scale.setScalar(0.75);

        ring2.rotation.x =
            Math.PI * 0.65;

        scene.add(ring2);

        // =========================================================
        // PARTICLE FIELD
        // =========================================================

        const particleCount =
            isMobile ? 120 : 450;

        const particlePositions =
            new Float32Array(
                particleCount * 3
            );

        for (
            let i = 0;
            i < particleCount;
            i++
        ) {

            particlePositions[i * 3] =
                (Math.random() - 0.5) * 32;

            particlePositions[i * 3 + 1] =
                (Math.random() - 0.5) * 17;

            particlePositions[i * 3 + 2] =
                -Math.random() * 18;
        }

        const particleGeometry =
            new THREE.BufferGeometry();

        particleGeometry.setAttribute(
            'position',
            new THREE.BufferAttribute(
                particlePositions,
                3
            )
        );

        const particleMaterial =
            new THREE.PointsMaterial({

                color: MUTED,

                size:
                    isMobile
                        ? 0.055
                        : 0.045,

                transparent: true,

                opacity: 0.38
            });

        const particles =
            new THREE.Points(
                particleGeometry,
                particleMaterial
            );

        scene.add(particles);

        // =========================================================
        // SMALL ACCENT PARTICLES
        // =========================================================

        const accentCount =
            isMobile ? 30 : 80;

        const accentPositions =
            new Float32Array(
                accentCount * 3
            );

        for (
            let i = 0;
            i < accentCount;
            i++
        ) {

            accentPositions[i * 3] =
                (Math.random() - 0.5) * 25;

            accentPositions[i * 3 + 1] =
                (Math.random() - 0.5) * 14;

            accentPositions[i * 3 + 2] =
                -4 - Math.random() * 12;
        }

        const accentGeometry =
            new THREE.BufferGeometry();

        accentGeometry.setAttribute(
            'position',
            new THREE.BufferAttribute(
                accentPositions,
                3
            )
        );

        const accentParticles =
            new THREE.Points(
                accentGeometry,
                new THREE.PointsMaterial({

                    color: ACCENT,

                    size: 0.065,

                    transparent: true,

                    opacity: 0.45
                })
            );

        scene.add(accentParticles);

        // =========================================================
        // MOUSE PARALLAX
        // =========================================================

        let targetX = 0;
        let targetY = 0;

        let currentX = 0;
        let currentY = 0;

        const hero =
            container.parentElement;

        hero.addEventListener(
            'pointermove',
            function (event) {

                const rect =
                    hero.getBoundingClientRect();

                targetX =
                    (
                        (event.clientX - rect.left)
                        / rect.width
                        - 0.5
                    ) * 2;

                targetY =
                    (
                        (event.clientY - rect.top)
                        / rect.height
                        - 0.5
                    ) * 2;
            }
        );

        hero.addEventListener(
            'pointerleave',
            function () {

                targetX = 0;
                targetY = 0;
            }
        );

        // =========================================================
        // RESIZE
        // =========================================================

        fit(
            renderer,
            camera,
            container
        );

        window.addEventListener(
            'resize',
            function () {

                fit(
                    renderer,
                    camera,
                    container
                );
            }
        );

        if (window.ResizeObserver) {

            new ResizeObserver(
                function () {

                    fit(
                        renderer,
                        camera,
                        container
                    );
                }
            ).observe(container);
        }

        // =========================================================
        // ANIMATION
        // =========================================================

        let running = true;
        let time = 0;

        onVisible(
            hero,
            function (visible) {

                running = visible;
            }
        );

        function animate() {

            requestAnimationFrame(
                animate
            );

            if (!running) return;

            const dt =
                reduceMotion
                    ? 0
                    : 0.016;

            time += dt;

            // -----------------------------------------------------
            // FLOATING SHAPES
            // -----------------------------------------------------

            objects.forEach(
                function (mesh) {

                    const u =
                        mesh.userData;

                    mesh.rotation.x +=
                        u.rotationX * dt;

                    mesh.rotation.y +=
                        u.rotationY * dt;

                    mesh.rotation.z +=
                        u.rotationZ * dt;

                    mesh.position.y =
                        u.baseY +
                        Math.sin(
                            time *
                            u.floatSpeed +
                            u.phase
                        ) *
                        u.floatAmplitude;

                    mesh.position.x =
                        u.baseX +
                        Math.sin(
                            time * 0.15 +
                            u.phase
                        ) *
                        0.12;
                }
            );

            // -----------------------------------------------------
            // RINGS
            // -----------------------------------------------------

            ring1.rotation.z +=
                dt * 0.08;

            ring1.rotation.y +=
                dt * 0.04;

            ring2.rotation.z -=
                dt * 0.05;

            ring2.rotation.x +=
                dt * 0.025;

            // -----------------------------------------------------
            // PARTICLES
            // -----------------------------------------------------

            particles.rotation.y +=
                dt * 0.008;

            particles.rotation.x =
                Math.sin(time * 0.08) *
                0.02;

            accentParticles.rotation.y -=
                dt * 0.012;

            // -----------------------------------------------------
            // CAMERA PARALLAX
            // -----------------------------------------------------

            currentX +=
                (targetX - currentX) *
                0.035;

            currentY +=
                (targetY - currentY) *
                0.035;

            camera.position.x =
                currentX * 1.1;

            camera.position.y =
                -currentY * 0.7;

            camera.lookAt(
                0,
                0,
                0
            );

            renderer.render(
                scene,
                camera
            );
        }

        animate();
    }

  /* ---------- Scene 2: Particle wave band ---------- */
  function initWave() {
    const container = document.getElementById('sceneWave');
    if (!container) return;

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(55, 1, 0.1, 200);
    camera.position.set(0, 7, 15);
    camera.lookAt(0, 0, 0);
    const renderer = makeRenderer(container);

    const COLS = isMobile ? 60 : 90, ROWS = isMobile ? 26 : 36, SEP = 0.42;
    const count = COLS * ROWS;
    const pos = new Float32Array(count * 3);
    const col = new Float32Array(count * 3);
    const ink = new THREE.Color(INK), acc = new THREE.Color(ACCENT);

    let k = 0;
    for (let i = 0; i < COLS; i++) {
      for (let j = 0; j < ROWS; j++) {
        pos[k * 3] = (i - COLS / 2) * SEP;
        pos[k * 3 + 1] = 0;
        pos[k * 3 + 2] = (j - ROWS / 2) * SEP;
        k++;
      }
    }
    const geo = new THREE.BufferGeometry();
    geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
    geo.setAttribute('color', new THREE.BufferAttribute(col, 3));
    const points = new THREE.Points(geo, new THREE.PointsMaterial({ size: 0.075, vertexColors: true, transparent: true, opacity: 0.95 }));
    scene.add(points);

    let mx = 0, targetMx = 0;
    container.addEventListener('pointermove', e => {
      const r = container.getBoundingClientRect();
      targetMx = ((e.clientX - r.left) / r.width - 0.5) * COLS * SEP;
    });

    let running = true, t = 0;
    onVisible(container, v => { running = v; });
    fit(renderer, camera, container);
    window.addEventListener('resize', () => fit(renderer, camera, container));

    const tmp = new THREE.Color();
    function frame() {
      requestAnimationFrame(frame);
      if (!running) return;
      t += reduceMotion ? 0 : 0.016;
      mx += (targetMx - mx) * 0.06;

      let n = 0;
      for (let i = 0; i < COLS; i++) {
        for (let j = 0; j < ROWS; j++) {
          const x = pos[n * 3];
          const z = pos[n * 3 + 2];
          const wave = Math.sin(x * 0.55 + t * 1.3) * 0.55 + Math.cos(z * 0.6 + t * 1.0) * 0.45;
          const d = Math.hypot(x - mx, z);
          const bump = Math.exp(-d * d * 0.06) * 1.6;
          const y = wave + bump;
          pos[n * 3 + 1] = y;

          const mix = Math.min(1, Math.max(0, (y + 0.6) / 2.6));
          tmp.copy(ink).lerp(acc, mix);
          col[n * 3] = tmp.r; col[n * 3 + 1] = tmp.g; col[n * 3 + 2] = tmp.b;
          n++;
        }
      }
      geo.attributes.position.needsUpdate = true;
      geo.attributes.color.needsUpdate = true;
      points.rotation.y = Math.sin(t * 0.15) * 0.08;

      renderer.render(scene, camera);
    }
    frame();
  }

  function start() {
    try { initHero(); } catch (e) { console.warn('Hero scene failed', e); }
    try { initWave(); } catch (e) { console.warn('Wave scene failed', e); }
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', start);
  else start();
})();
