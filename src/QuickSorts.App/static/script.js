(async function () {
  document.addEventListener('keydown', async function (event) {
    if (event.code === 'Space' || event.key === ' ') {
      console.log('Spacebar was pressed');
      await handleDeny();
    }
    else if (event.key == 'ArrowLeft') {
      console.log('ArrowLeft was pressed');
      await handlePrev();
    }
    else if (event.key == 'ArrowRight') {
      console.log('Arrowright was pressed');
      await handleConfirm();
    }

    await handleProgress();
  });
  const btnConfirmId = "btnConfirm";
  const btnPrevId = "btnPrev";
  const btnDenyId = "btnDeny";
  const imageId = "imgMain";
  const headerId = "hdrTitle"
  const progressId = "hdrProgress";
  document.getElementById(btnConfirmId).addEventListener('click', async (event) => {
    event.preventDefault();
    console.log(`btnConfirmId: ${btnConfirmId} was clicked`);
    handleConfirm();
  });
  document.getElementById(btnPrevId).addEventListener('click', async (event) => {
    event.preventDefault();
    console.log(`btnPrevId: ${btnPrevId} was clicked`);
    handlePrev();
  });
  document.getElementById(btnDenyId).addEventListener('click', async (event) => {
    event.preventDefault();
    console.log(`btnDenyId: ${btnDenyId} was clicked`);
    handleDeny();
  });

  const setProgressParagraph = (progressId, text) => document.getElementById(progressId).textContent = text;
  const setImageSrc = (imageId, url) => document.getElementById(imageId).src = url;
  const setImageTitle = (headerId, imageTitle) => document.getElementById(headerId).textContent = imageTitle;
  const handleConfirm = async () => {
    const imageUrl = await fetch("/next?action=confirm")
      .then((response) => response.text())
      .catch(err => {
        console.error(err)
      });
    console.log(`confirm - imageUrl: /${imageUrl}`);
    setImageSrc(imageId, "/" + imageUrl);
    setImageTitle(headerId, imageUrl);
  };
  const handleDeny = async () => {
    const imageUrl = await fetch("/next?action=deny")
      .then((response) => response.text())
      .catch(err => {
        console.error(err)
      });
    console.log(`deny - imageUrl: /${imageUrl}`);
    setImageSrc(imageId, "/" + imageUrl);
    setImageTitle(headerId, imageUrl);
  };
  const handlePrev = async () => {
    await fetch("/prev")
      .catch(err => {
        console.error(err)
      });
    await handleCurr();
  };
  const handleCurr = async () => {
    const imageUrl = await fetch("/curr")
      .then((response) => response.text())
      .catch(err => {
        console.error(err)
      });
    console.log(`curr - imageUrl: /${imageUrl}`);
    setImageSrc(imageId, "/" + imageUrl);
    setImageTitle(headerId, imageUrl);
  }
  const handleProgress = async () => {
    const fetchTotal = await fetch("/total").then(r => r.text()).catch(err => console.log(err));
    const fetchProgress = await fetch("/progress").then(r => r.text()).catch(err => console.log(err));
    setProgressParagraph(progressId, `${fetchProgress} / ${fetchTotal}`);
    console.log(`progress ${fetchProgress} / ${fetchTotal}`);
  }
  // startup
  await handleCurr();
  await handleProgress();
})();
