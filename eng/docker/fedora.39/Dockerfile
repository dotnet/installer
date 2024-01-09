#
# Copyright (c) .NET Foundation and contributors. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
#

# Dockerfile that creates a container suitable to build dotnet-cli
FROM mcr.microsoft.com/dotnet-buildtools/prereqs:fedora-39-20231212232848-7d0f9b0

RUN dnf install -y nss

RUN dnf clean all

# Install ICU
RUN mkdir /icu && cd /icu \
 && curl -sL https://github.com/unicode-org/icu/releases/download/release-62-1/icu4c-62_1-src.tgz -o icu4c.tgz \
 && tar xf icu4c.tgz \
 && ls && cd icu \
 && ./source/runConfigureICU Linux \
 && make -j \
 && make install \
 && cd / && rm -r /icu

# Override RID set by the dotnet-buildtools-prereqs docker image
ENV __PUBLISH_RID=fedora.39-x64

# Setup User to match Host User, and give superuser permissions 
ARG USER_ID=0 
RUN useradd -m code_executor -u ${USER_ID} -g wheel
RUN echo 'code_executor ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers 
 
# With the User Change, we need to change permissions on these directories 
RUN chmod -R a+rwx /usr/local 
RUN chmod -R a+rwx /home
 
# Set user to the one we just created 
USER ${USER_ID} 
 
# Set working directory 
ARG WORK_DIR 
WORKDIR ${WORK_DIR}
